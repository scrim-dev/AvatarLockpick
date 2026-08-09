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
	"net/url"
	"os"
	"os/exec"
	"path/filepath"
	"strconv"
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
	releaseAPIBase = "https://api.github.com/repos/" + repository + "/releases/tags/"
	latestAPIURL   = "https://api.github.com/repos/" + repository + "/releases/latest"
	releasesAPIURL = "https://api.github.com/repos/" + repository + "/releases?per_page=20"
	tagsAPIURL     = "https://api.github.com/repos/" + repository + "/tags?per_page=20"
	userAgent      = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:141.0) Gecko/20100101 Firefox/141.0"
)

var (
	packageURL = "https://raw.githubusercontent.com/" + repository + "/master/_binary/AvatarLockpick-win-x64.zip"
	devMode    = "false"
)

//go:embed Ins_icon.ico
var installerIcon []byte

type remoteInfo struct {
	version    string
	published  bool
	preRelease bool
	tagExists  bool
}

func main() {
	a := app.NewWithID("com.scrimdev.avatarlockpick.installer")
	title := "AvatarLockpick Installer"
	if isDevMode() {
		title += " (Dev Mode)"
	}
	w := a.NewWindow(title)
	w.Resize(fyne.NewSize(580, 330))
	w.SetIcon(fyne.NewStaticResource("Ins_icon.ico", installerIcon))

	statusText := "Checking for the latest release..."
	if isDevMode() {
		statusText = "Dev mode: checking release info. Installer will download the test file only."
	}
	status := widget.NewLabel(statusText)
	status.Wrapping = fyne.TextWrapWord
	installDesktop := widget.NewCheck("Create a desktop shortcut", nil)
	installButton := widget.NewButton("Install", nil)
	installButton.Disable()
	progress := widget.NewProgressBar()
	progress.Hide()

	var info remoteInfo
	installButton.OnTapped = func() {
		if !info.published {
			message := fmt.Sprintf("Version %s does not have a GitHub Release. It is an unpublished pre-release. Continue?", info.version)
			if info.tagExists {
				message = fmt.Sprintf("Version %s has a GitHub tag, but no GitHub Release was published for it. Continue?", info.version)
			}
			dialog.ShowConfirm("Pre-release build", message, func(ok bool) {
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
		if !loaded.published && loaded.tagExists {
			message += "\nWarning: this version has a GitHub tag but no GitHub Release."
		} else if !loaded.published {
			message += "\nWarning: this is an unpublished pre-release."
		}
		if isDevMode() {
			message += "\nDev mode: download URL is " + packageURL
			message += "\nNo files will be installed and no shortcuts will be changed."
		}
		runOnUI(func() {
			info = loaded
			if isDevMode() {
				installButton.SetText("Download test file")
			} else if local == "" {
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
	if isDevMode() {
		setStatus(status, "Downloading dev test file...")
	} else {
		setStatus(status, "Downloading AvatarLockpick "+info.version+"...")
	}
	go func() {
		installedExe, err := downloadAndInstall(info.version, desktop, func(value float64) {
			setProgress(progress, value)
		})
		if err != nil {
			setStatus(status, "Installation failed: "+err.Error())
			enableButton(button)
			return
		}
		setProgress(progress, 1)
		if isDevMode() {
			setStatus(status, "Dev download test completed successfully.")
		} else {
			setStatus(status, "Installed "+info.version+" successfully.")
		}
		runOnUI(func() {
			if isDevMode() {
				dialog.ShowInformation("Dev download complete", "The dev test file downloaded successfully. Nothing was installed.", w)
				return
			}
			dialog.ShowConfirm("Installed", "AvatarLockpick was installed. Launch it now?", func(ok bool) {
				if !ok {
					return
				}
				if err := launchApp(installedExe); err != nil {
					dialog.ShowError(err, w)
				}
			}, w)
		})
	}()
}

func getRemoteInfo() (remoteInfo, error) {
	if isDevMode() {
		return remoteInfo{version: "dev-download-test", published: true}, nil
	}

	client := &http.Client{Timeout: 30 * time.Second}
	response, err := get(client, versionURL)
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

	if latest, ok := latestPublishedVersion(client); ok && compareVersions(latest, version) > 0 {
		version = latest
	}

	info := remoteInfo{version: version}
	release, err := get(client, releaseAPIBase+url.PathEscape(version))
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
	if !info.published {
		if published, prerelease := releaseExistsForVersionDate(client, version); published {
			info.published, info.preRelease = true, prerelease
		}
	}
	if !info.published {
		info.tagExists = tagExists(client, version)
	}
	return info, nil
}

func downloadAndInstall(version string, desktop bool, report func(float64)) (string, error) {
	client := &http.Client{Timeout: 5 * time.Minute}
	response, err := get(client, packageURL)
	if err != nil {
		return "", fmt.Errorf("could not download package: %w", err)
	}
	defer response.Body.Close()
	if response.StatusCode != http.StatusOK {
		return "", fmt.Errorf("package returned HTTP %d", response.StatusCode)
	}
	data, err := io.ReadAll(io.LimitReader(response.Body, 1024*1024*1024))
	if err != nil {
		return "", err
	}
	report(.45)
	if isDevMode() {
		if len(data) == 0 {
			return "", errors.New("dev test download was empty")
		}
		report(1)
		return "", nil
	}

	staging, err := os.MkdirTemp("", "AvatarLockpick-install-*")
	if err != nil {
		return "", err
	}
	defer os.RemoveAll(staging)
	if err := extractZip(data, staging); err != nil {
		return "", fmt.Errorf("invalid package: %w", err)
	}
	if _, err := os.Stat(filepath.Join(staging, "AvatarLockpick.exe")); err != nil {
		return "", errors.New("package does not contain AvatarLockpick.exe")
	}
	if info, err := os.Stat(filepath.Join(staging, "UI")); err != nil || !info.IsDir() {
		return "", errors.New("package does not contain the UI folder")
	}
	report(.7)

	target := filepath.Join(os.Getenv("LOCALAPPDATA"), "Programs", appName)
	if os.Getenv("LOCALAPPDATA") == "" {
		return "", errors.New("LOCALAPPDATA is not available")
	}
	backup := target + ".previous"
	_ = os.RemoveAll(backup)
	if _, err := os.Stat(target); err == nil {
		if err := os.Rename(target, backup); err != nil {
			return "", fmt.Errorf("close AvatarLockpick and retry: %w", err)
		}
	}
	if err := os.MkdirAll(filepath.Dir(target), 0755); err != nil {
		return "", err
	}
	if err := os.Rename(staging, target); err != nil {
		_ = os.Rename(backup, target)
		return "", err
	}
	installedExe := filepath.Join(target, "AvatarLockpick.exe")
	if err := createShortcut(startMenuShortcut(), installedExe); err != nil {
		return "", err
	}
	if desktop {
		if err := createShortcut(filepath.Join(os.Getenv("USERPROFILE"), "Desktop", "AvatarLockpick.lnk"), installedExe); err != nil {
			return "", err
		}
	} else {
		// Honor a user's choice not to have a desktop shortcut on reinstalls too.
		_ = os.Remove(filepath.Join(os.Getenv("USERPROFILE"), "Desktop", "AvatarLockpick.lnk"))
	}
	report(1)
	return installedExe, nil
}

func get(client *http.Client, url string) (*http.Response, error) {
	request, err := http.NewRequest(http.MethodGet, url, nil)
	if err != nil {
		return nil, err
	}
	request.Header.Set("User-Agent", userAgent)
	request.Header.Set("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8")
	request.Header.Set("Accept-Language", "en-US,en;q=0.5")
	return client.Do(request)
}

func latestPublishedVersion(client *http.Client) (string, bool) {
	best := ""
	if response, err := get(client, latestAPIURL); err == nil {
		defer response.Body.Close()
		if response.StatusCode == http.StatusOK {
			var release struct {
				TagName string `json:"tag_name"`
			}
			if json.NewDecoder(response.Body).Decode(&release) == nil && isCalendarVersion(release.TagName) {
				best = release.TagName
			}
		}
	}

	if response, err := get(client, tagsAPIURL); err == nil {
		defer response.Body.Close()
		if response.StatusCode == http.StatusOK {
			var tags []struct {
				Name string `json:"name"`
			}
			if json.NewDecoder(response.Body).Decode(&tags) == nil {
				for _, tag := range tags {
					if isCalendarVersion(tag.Name) && compareVersions(tag.Name, best) > 0 {
						best = tag.Name
					}
				}
			}
		}
	}

	return best, best != ""
}

func releaseExistsForVersionDate(client *http.Client, version string) (bool, bool) {
	date, ok := versionDate(version)
	if !ok {
		return false, false
	}

	response, err := get(client, releasesAPIURL)
	if err != nil {
		return false, false
	}
	defer response.Body.Close()
	if response.StatusCode != http.StatusOK {
		return false, false
	}

	var releases []struct {
		Name       string `json:"name"`
		TagName    string `json:"tag_name"`
		Prerelease bool   `json:"prerelease"`
	}
	if json.NewDecoder(response.Body).Decode(&releases) != nil {
		return false, false
	}
	for _, release := range releases {
		if releaseMatchesDate(release.Name, date) || releaseMatchesDate(release.TagName, date) {
			return true, release.Prerelease
		}
	}
	return false, false
}

func releaseMatchesDate(value, date string) bool {
	normalized := strings.TrimSpace(strings.ToLower(value))
	normalized = strings.TrimPrefix(normalized, "version ")
	return normalized == date
}

func tagExists(client *http.Client, version string) bool {
	response, err := get(client, "https://api.github.com/repos/"+repository+"/git/ref/tags/"+url.PathEscape(version))
	if err != nil {
		return false
	}
	defer response.Body.Close()
	return response.StatusCode == http.StatusOK
}

func compareVersions(left, right string) int {
	leftParts, leftOK := parseCalendarVersion(left)
	rightParts, rightOK := parseCalendarVersion(right)
	if !leftOK && !rightOK {
		return 0
	}
	if leftOK && !rightOK {
		return 1
	}
	if !leftOK && rightOK {
		return -1
	}
	for index := range leftParts {
		if leftParts[index] > rightParts[index] {
			return 1
		}
		if leftParts[index] < rightParts[index] {
			return -1
		}
	}
	return 0
}

func isCalendarVersion(value string) bool {
	_, ok := parseCalendarVersion(value)
	return ok
}

func versionDate(value string) (string, bool) {
	dateAndBuild := strings.SplitN(strings.TrimSpace(value), "-", 2)
	if len(dateAndBuild) != 2 {
		return "", false
	}
	if _, ok := parseCalendarVersion(value); !ok {
		return "", false
	}
	return dateAndBuild[0], true
}

func parseCalendarVersion(value string) ([4]int, bool) {
	var result [4]int
	dateAndBuild := strings.SplitN(strings.TrimSpace(value), "-", 2)
	if len(dateAndBuild) != 2 {
		return result, false
	}
	dateParts := strings.Split(dateAndBuild[0], ".")
	if len(dateParts) != 3 {
		return result, false
	}
	for index, part := range dateParts {
		number, err := strconv.Atoi(part)
		if err != nil {
			return result, false
		}
		result[index] = number
	}
	build, err := strconv.Atoi(dateAndBuild[1])
	if err != nil {
		return result, false
	}
	result[3] = build
	return result, true
}

func launchApp(path string) error {
	workingDirectory := filepath.Dir(path)
	if info, err := os.Stat(filepath.Join(workingDirectory, "UI")); err != nil || !info.IsDir() {
		return errors.New("could not launch AvatarLockpick because the installed UI folder was not found")
	}
	command := exec.Command(path)
	command.Dir = workingDirectory
	if err := command.Start(); err != nil {
		return fmt.Errorf("could not launch AvatarLockpick: %w", err)
	}
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

func isDevMode() bool {
	return strings.EqualFold(devMode, "true") || devMode == "1"
}
