﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ICSharpCode.ILSpy
{
	static class NativeMethods
	{
		public const uint WM_COPYDATA = 0x4a;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern unsafe int GetWindowThreadProcessId(IntPtr hWnd, int* lpdwProcessId);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder title, int size);

		public static string GetWindowText(IntPtr hWnd, int maxLength)
		{
			StringBuilder b = new StringBuilder(maxLength + 1);
			if (GetWindowText(hWnd, b, b.Capacity) != 0)
				return b.ToString();
			else
				return string.Empty;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern IntPtr SendMessageTimeout(
			IntPtr hWnd, uint msg, IntPtr wParam, ref CopyDataStruct lParam,
			uint flags, uint timeout, out IntPtr result);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetForegroundWindow(IntPtr hWnd);

		public unsafe static string GetProcessNameFromWindow(IntPtr hWnd)
		{
			int processId;
			GetWindowThreadProcessId(hWnd, &processId);
			try
			{
				using (var p = Process.GetProcessById(processId))
				{
					return p.ProcessName;
				}
			}
			catch (ArgumentException ex)
			{
				Debug.WriteLine(ex.Message);
				return null;
			}
			catch (InvalidOperationException ex)
			{
				Debug.WriteLine(ex.Message);
				return null;
			}
			catch (Win32Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return null;
			}
		}

		[DllImport("dwmapi.dll", PreserveSig = true)]
		public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

		public static bool UseImmersiveDarkMode(IntPtr hWnd, bool enable)
		{
			int darkMode = enable ? 1 : 0;
			int hr = DwmSetWindowAttribute(hWnd, DwmWindowAttribute.UseImmersiveDarkMode, ref darkMode, sizeof(int));
			return hr >= 0;
		}
	}

	[return: MarshalAs(UnmanagedType.Bool)]
	delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	[StructLayout(LayoutKind.Sequential)]
	struct CopyDataStruct
	{
		public IntPtr Padding;
		public int Size;
		public IntPtr Buffer;

		public CopyDataStruct(IntPtr padding, int size, IntPtr buffer)
		{
			this.Padding = padding;
			this.Size = size;
			this.Buffer = buffer;
		}
	}

	public enum DwmWindowAttribute : uint
	{
		NCRenderingEnabled = 1,
		NCRenderingPolicy,
		TransitionsForceDisabled,
		AllowNCPaint,
		CaptionButtonBounds,
		NonClientRtlLayout,
		ForceIconicRepresentation,
		Flip3DPolicy,
		ExtendedFrameBounds,
		HasIconicBitmap,
		DisallowPeek,
		ExcludedFromPeek,
		Cloak,
		Cloaked,
		FreezeRepresentation,
		PassiveUpdateMode,
		UseHostBackdropBrush,
		UseImmersiveDarkMode = 20,
		WindowCornerPreference = 33,
		BorderColor,
		CaptionColor,
		TextColor,
		VisibleFrameBorderThickness,
		SystemBackdropType,
		Last
	}
}
