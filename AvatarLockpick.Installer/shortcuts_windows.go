//go:build windows

package main

import (
	"os"
	"path/filepath"

	"github.com/go-ole/go-ole"
	"github.com/go-ole/go-ole/oleutil"
)

func createShortcut(path, target string) error {
	if err := os.MkdirAll(filepath.Dir(path), 0755); err != nil { return err }
	_ = os.Remove(path) // Always replace an existing Start Menu shortcut.
	if err := ole.CoInitialize(0); err != nil { return err }
	defer ole.CoUninitialize()
	shell, err := oleutil.CreateObject("WScript.Shell")
	if err != nil { return err }
	defer shell.Release()
	dispatch, err := shell.QueryInterface(ole.IID_IDispatch)
	if err != nil { return err }
	defer dispatch.Release()
	shortcut, err := oleutil.CallMethod(dispatch, "CreateShortcut", path)
	if err != nil { return err }
	link := shortcut.ToIDispatch()
	defer link.Release()
	if _, err = oleutil.PutProperty(link, "TargetPath", target); err != nil { return err }
	if _, err = oleutil.PutProperty(link, "WorkingDirectory", filepath.Dir(target)); err != nil { return err }
	if _, err = oleutil.PutProperty(link, "IconLocation", target+",0"); err != nil { return err }
	_, err = oleutil.CallMethod(link, "Save")
	return err
}
