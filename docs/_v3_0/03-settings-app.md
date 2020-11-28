---
title: The Settings App
permalink: /v3.0/settings-app
toc: false
---

Keyboard Switch Settings is a simple app which controls the behavior of the Keyboard Switch service. If you installed
the app using a Windows installer, you can find the app in the Start menu.

Here's how it looks:

![app-screen](/assets/images/v3.0/screen-char-mappings.png)

The app is composed of six tabs:

- Character mappings: contains the settings which tell Keyboard Switch how to transform characters between layouts
- Preferences: contains other settings
- Converter: lets you manually convert text between arbitrary custom layouts
- Converter settings: contains the list of custom layouts for the converter
- Sandbox: lets you play around with switching the text
- About: contains info about the app

At the bottom there's a strip with the status of the Keyboard Switch service. If it's not running, you can press the
'Start' button to start it manually (although usually you wouldn't do that because the service will start when you log
in). If the service is running, you can stop it by pressing the 'Stop' button. It won't actually kill it - it will send
a 'stop' command, and the service will do some clean-up and stop on its own. If it's taking a long time to stop, it may
mean that something went wrong. In that case you can force it to stop by pressing the 'Kill' button.

**Note:** If you change anything in the settings app, you can save it or cancel it. When you save it, the settings app
sends a notification to the service app (if it's running) about changes in configuration so you don't have to restart
the service after saving.
{: .notice--primary}

The following articles describe each tab of the settings app in more detail.
