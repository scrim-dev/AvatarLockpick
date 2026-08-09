//go:build !windows

package main

import "errors"

func createShortcut(_, _ string) error { return errors.New("shortcuts are only supported on Windows") }
