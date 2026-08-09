package main

import (
	"archive/zip"
	"bytes"
	_ "embed"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"strings"
	"time"

	"fyne.io/fyne/v2"
	"fyne.io/fyne/v2/app"
	"fyne.io/fyne/v2/container"
	"fyne.io/fyne/v2/dialog"
	"fyne.io/fyne/v2/widget"
)

const (
	appName        = "AvatarLockpick"
	repository     = "scrim-dev/AvatarLockpick"
	versionURL     = "https://raw.githubusercontent.com/" + repository + "/master/version.txt"
	packageURL     = "https://raw.githubusercontent.com/" + repository + "/master/_binary/AvatarLockpick-win-x64.zip"
	releaseAPIBase = "https://api.github.com/repos/" + repository + "/releases/tags/"
)

//go:embed Ins_icon.ico
var installerIcon []byte

type remoteInfo struct {
	version    string
	published  bool
	preRelease bool
}

func main() {
	a := app.NewWithID("com.scrimdev.avatarlockpick.installer")
	w := a.NewWindow("AvatarLockpick Installer")
	w.Resize(fyne.NewSize(580, 330))
	w.SetIcon(fyne.NewStaticResource("Ins_icon.ico", installerIcon))

	status := widget.NewLabel("Checking for the latest release...")
	status.Wrapping = fyne.TextWrapWord
	installDesktop := widget.NewCheck("Create a desktop shortcut", nil)
	installButton := widget.NewButton("Install", nil)
	installButton.Disable()
	progress := widget.NewProgressBar()
	progress.Hide()

	var info remoteInfo
	installButton.OnTapped = func() {
		if !info.published {
			dialog.ShowConfirm("Pre-release build", fmt.Sprintf("Version %s does not have a GitHub Release. It is an unpublished pre-release. Continue?", info.version), func(ok bool) {
				if ok {
					install(w, status, progress, installButton, info, installDesktop.Checked)
				}
			}, w)
			return
		}
		install(w, status, progress, installButton, info, installDesktop.Checked)
	}

	w.SetContent(container.NewVBox(
		widget.NewLabelWithStyle("AvatarLockpick", fyne.TextAlignCenter, fyne.TextStyle{Bold: true}),
		widget.NewSeparator(),
		status,
		installDesktop,
		progress,
		installButton,
	))
	go func() {
		loaded, err := getRemoteInfo()
		if err != nil {
			setStatus(status, "Unable to check for updates: "+err.Error())
			return
		}
		local := installedVersion(filepath.Join(os.Getenv("LOCALAPPDATA"), "Programs", appName, "AvatarLockpick.exe"))
		message := "Latest version: " + loaded.version
		if local == "" {
			message += "\nAvatarLockpick is not installed."
		} else if local == loaded.version {
			message += "\nInstalled version: " + local + " (current)"
		} else {
			message += "\nInstalled version: " + local + " (update available)"
		}
		if !loaded.published {
			message += "\nWarning: this is an unpublished pre-release."
		}
		runOnUI(func() {
			info = loaded
			if local == "" {
				installButton.SetText("Install")
			} else if local == loaded.version {
				installButton.SetText("Reinstall")
			} else {
				installButton.SetText("Update")
			}
			status.SetText(message)
			installButton.Enable()
		})
	}()
	w.ShowAndRun()
}

func install(w fyne.Window, status *widget.Label, progress *widget.ProgressBar, button *widget.Button, info remoteInfo, desktop bool) {
	button.Disable()
	progress.Show()
	progress.SetValue(0)
	setStatus(status, "Downloading AvatarLockpick "+info.version+"...")
	go func() {
		err := downloadAndInstall(info.version, desktop, func(value float64) {
			setProgress(progress, value)
		})
		if err != nil {
			setStatus(status, "Installation failed: "+err.Error())
			enableButton(button)
			return
		}
		setProgress(progress, 1)
		setStatus(status, "Installed "+info.version+" successfully.")
		runOnUI(func() {
			dialog.ShowInformation("Installed", "AvatarLockpick was installed. A Start Menu shortcut was created.", w)
		})
	}()
}

func getRemoteInfo() (remoteInfo, error) {
	client := &http.Client{Timeout: 30 * time.Second}
	response, err := client.Get(versionURL)
	if err != nil {
		return remoteInfo{}, fmt.Errorf("could not read version.txt: %w", err)
	}
	defer response.Body.Close()
	if response.StatusCode != http.StatusOK {
		return remoteInfo{}, fmt.Errorf("version.txt returned HTTP %d", response.StatusCode)
	}
	body, err := io.ReadAll(io.LimitReader(response.Body, 1024))
	if err != nil {
		return remoteInfo{}, err
	}
	version := strings.TrimSpace(string(body))
	if version == "" {
		return remoteInfo{}, errors.New("version.txt is empty")
	}

	info := remoteInfo{version: version}
	release, err := client.Get(releaseAPIBase + version)
	if err != nil {
		return info, nil
	} // GitHub availability must not prevent a package install.
	defer release.Body.Close()
	if release.StatusCode == http.StatusOK {
		var metadata struct {
			Prerelease bool `json:"prerelease"`
		}
		if json.NewDecoder(release.Body).Decode(&metadata) == nil {
			info.published, info.preRelease = true, metadata.Prerelease
		}
	}
	return info, nil
}

func downloadAndInstall(version string, desktop bool, report func(float64)) error {
	client := &http.Client{Timeout: 5 * time.Minute}
	response, err := client.Get(packageURL)
	if err != nil {
		return fmt.Errorf("could not download package: %w", err)
	}
	defer response.Body.Close()
	if response.StatusCode != http.StatusOK {
		return fmt.Errorf("package returned HTTP %d", response.StatusCode)
	}
	data, err := io.ReadAll(io.LimitReader(response.Body, 1024*1024*1024))
	if err != nil {
		return err
	}
	report(.45)

	staging, err := os.MkdirTemp("", "AvatarLockpick-install-*")
	if err != nil {
		return err
	}
	defer os.RemoveAll(staging)
	if err := extractZip(data, staging); err != nil {
		return fmt.Errorf("invalid package: %w", err)
	}
	if _, err := os.Stat(filepath.Join(staging, "AvatarLockpick.exe")); err != nil {
		return errors.New("package does not contain AvatarLockpick.exe")
	}
	report(.7)

	target := filepath.Join(os.Getenv("LOCALAPPDATA"), "Programs", appName)
	if os.Getenv("LOCALAPPDATA") == "" {
		return errors.New("LOCALAPPDATA is not available")
	}
	backup := target + ".previous"
	_ = os.RemoveAll(backup)
	if _, err := os.Stat(target); err == nil {
		if err := os.Rename(target, backup); err != nil {
			return fmt.Errorf("close AvatarLockpick and retry: %w", err)
		}
	}
	if err := os.MkdirAll(filepath.Dir(target), 0755); err != nil {
		return err
	}
	if err := os.Rename(staging, target); err != nil {
		_ = os.Rename(backup, target)
		return err
	}
	if err := createShortcut(startMenuShortcut(), filepath.Join(target, "AvatarLockpick.exe")); err != nil {
		return err
	}
	if desktop {
		if err := createShortcut(filepath.Join(os.Getenv("USERPROFILE"), "Desktop", "AvatarLockpick.lnk"), filepath.Join(target, "AvatarLockpick.exe")); err != nil {
			return err
		}
	} else {
		// Honor a user's choice not to have a desktop shortcut on reinstalls too.
		_ = os.Remove(filepath.Join(os.Getenv("USERPROFILE"), "Desktop", "AvatarLockpick.lnk"))
	}
	report(1)
	return nil
}

func extractZip(data []byte, destination string) error {
	reader, err := zip.NewReader(bytes.NewReader(data), int64(len(data)))
	if err != nil {
		return err
	}
	root := filepath.Clean(destination) + string(os.PathSeparator)
	for _, file := range reader.File {
		name := filepath.Join(destination, file.Name)
		if !strings.HasPrefix(filepath.Clean(name)+func() string {
			if file.FileInfo().IsDir() {
				return string(os.PathSeparator)
			}
			return ""
		}(), root) && filepath.Clean(name) != strings.TrimSuffix(root, string(os.PathSeparator)) {
			return errors.New("zip contains an unsafe path")
		}
		if file.FileInfo().IsDir() {
			if err := os.MkdirAll(name, 0755); err != nil {
				return err
			}
			continue
		}
		if err := os.MkdirAll(filepath.Dir(name), 0755); err != nil {
			return err
		}
		in, err := file.Open()
		if err != nil {
			return err
		}
		out, err := os.OpenFile(name, os.O_CREATE|os.O_TRUNC|os.O_WRONLY, file.Mode())
		if err != nil {
			in.Close()
			return err
		}
		_, copyErr := io.Copy(out, in)
		out.Close()
		in.Close()
		if copyErr != nil {
			return copyErr
		}
	}
	return nil
}

func startMenuShortcut() string {
	return filepath.Join(os.Getenv("APPDATA"), "Microsoft", "Windows", "Start Menu", "Programs", "AvatarLockpick.lnk")
}

func setStatus(label *widget.Label, value string) {
	runOnUI(func() {
		label.SetText(value)
	})
}

func setProgress(progress *widget.ProgressBar, value float64) {
	runOnUI(func() {
		progress.SetValue(value)
	})
}

func enableButton(button *widget.Button) {
	runOnUI(func() {
		button.Enable()
	})
}

func runOnUI(update func()) {
	fyne.Do(update)
}
