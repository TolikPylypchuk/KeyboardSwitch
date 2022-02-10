# Keyboard Switch

Welcome to the Keyboard Switch docs!

Keyboard Switch is an application which switches typed text as if it were typed with another keyboard layout.

If you ever write some text and then realize that you have written it using a wrong layout, you don't have to delete it and start over anymore. Just select the text, press a magic key combination, and that's it!

I hope this app will make your life at least 1% easier :)

{% hint style="info" %}
If you're using version 4.0, then you should upgrade to version 4.1 as soon as possible since it contains several improvements and bug fixes.
{% endhint %}

## Quick Start

### Windows

You can get the latest version of the app from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases). Download the _.msi_ file and run it to install the app.

After installation, the Keyboard Switch Settings app will start. It may take some time as the app will do some initial setup. If it doesn't start for some reason, then find it in the list of your apps.

In the opened app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to configure it further. But if you do, then all the info you'll ever need is provided in these docs.

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_ twice instead.

### Linux

The app is available as a deb package (for Debian-based distributions, such as Ubuntu, Mint etc.), an RPM package (for RHEL-based distributions, like CentOS or Fedora, as well as SUSE), and a simple _tar.gz_ file. You can get these files from the [releases page on GitHub](https://github.com/TolikPylypchuk/KeyboardSwitch/releases).

{% hint style="info" %}
If you're using an RPM package there are a couple things to note before installing the app (which you can find in the [installation](usage-guides/installation.md#installing-the-app) article).
{% endhint %}

Bear in mind that KeyboardSwitch only works on X11 - it won't work on Wayland (even with XWayland apparently).

If you use the deb or RPM package, then simply install it either by double-clicking on it, or through the terminal. If you use the _tar.gz_ file, then the set-up is not quite as quick, so you can read about it in the [installation page](usage-guides/installation.md#installing-the-app).

After installing the app, open Keyboard Switch Settings - it should appear in the list of your apps.

In the opened app press the _Auto-configure_ button.

Press _Save_.

Press _Start_.

That's it! You're ready to use Keyboard Switch in it's basic configuration. There's a big chance you won't need to configure it further. But if you do, then all the info you'll ever need is provided in these docs.

To switch text forward, select the text, and press _Ctrl+Shift_ twice. To switch it backward, press _Ctrl+Alt+Shift_ twice instead.

## How the App Works

The app is composed of two parts: the Keyboard Switch service and the Keyboard Switch Settings app. The service always runs in the background (and starts when you log into the system) and listens to key presses. When you press the magic key combination, it reacts by copying the selected text, transforming it, and pasting it for you. The settings app is used for configuring the service and for starting/stopping it manually.

Here are the basic steps you should take to switch the text:

* Type some text using an incorrect layout
* Realize your mistake
* Select the text (you can press _Ctrl+A_ to select all text)
* Press the magic key combination (the default is pressing _Ctrl+Shift_ twice)
* Profit

The app uses the clipboard to get the text to transform, and then puts the transformed text back into the clipboard. The app will try to preserve the text that was in the clipboard before switching, and restoring it afterwards. It doesn't guarantee that the text will be restored though. Also, any non-text data (e.g. a file or a picture) will not be restored.

{% hint style="info" %}
**Note:** The app simulates pressing _Ctrl+C_ to copy and _Ctrl+V_ to paste text. This is the default behavior for most (if not all) applications. But if you are working in an app which behaves differently on those key presses, this app will not be able to instantly switch text. You can disable this behavior, and copy/paste the text manually.
{% endhint %}

## Supported Platforms

Keyboard Switch currently works on Windows 10/11 and on Linux via X11. It can probably work on older versions of Windows as well, but I'm not going to support them. As for Linux, it needs several prerequisites to run (more on that on [the installation page](usage-guides/installation.md#linux)).

Only the x64 architecture is supported. It would be ideal to also support Arm64, but it's unlikely in the nearest future.

## Layouts

The app uses the list of your layouts in the same order as defined by the system. You can switch both forward and backward through this list. The app also automatically changes your layouts, so you don't have to do it yourself. You can disable this as well if you so wish. You cannot add a layout that's not present as one of the system's layouts. If you add/remove a layout while the app is running, you'll have configure it in the settings and restart the service.

{% hint style="info" %}
**Note:** This app is "Western-oriented". I developed it specifically to handle switching between Cyrillic and Latin scripts. I don't know how it will work (if at all) with Eastern languages/scripts.
{% endhint %}
