//go:build windows

package main

import (
	"os"
	"os/exec"
	"strings"
)

// installedVersion reads the binary's Windows ProductVersion, which .NET populates from
// InformationalVersion. It deliberately avoids a separate installer state file.
func installedVersion(path string) string {
	if _, err := os.Stat(path); err != nil {
		return ""
	}
	command := "(Get-Item -LiteralPath '" + strings.ReplaceAll(path, "'", "''") + "').VersionInfo.ProductVersion"
	output, err := exec.Command("powershell.exe", "-NoProfile", "-NonInteractive", "-Command", command).Output()
	if err != nil {
		return ""
	}
	return strings.TrimSpace(string(output))
}
