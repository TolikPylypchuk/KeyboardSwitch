# Character Mappings

In order for the Keyboard Switch service to know how to map characters, you have to configure the character mappings yourself. The app reads the list of installed layouts in the system, so it knows which layouts you use and in what order. What it doesn't (and can't) know is the physical layout of your keyboard, so it doesn't know what happens if you press certain keys on your keyboard.

## Text vs Keyboard

This section explains _why_ you need to configure character mappings for the app. If you're not interested, feel free to skip it.

Working with text and working with keyboard are two wildly different things. You, the user of a computer, usually work with a keyboard (or other input devices, but those don't matter here). This app however works with text (even though its name might suggest differently), and so text transformation during switching will most probably not be perfect.

Ideally you wouldn't have to configure the app at all - it would just know how to transform the text you mistyped into another layout. After all, the OS somehow knows which characters to produce when you press various keys! But going that way would be extremely difficult, if not impossible.

Firstly, pressing keys and entering characters are absolutely not the same thing. Many keys don't have corresponding characters at all, like _Ctrl_ or _Page Up_. The same key press can produce different characters based on the state of other keys. For example, the _A_ key usually produces **a**, but when _Shift_ is down, the key produces **A**. And then there are dead keys - key combinations that produce one character by pressing two (or even three) keys. For example, in the US International layout, pressing\_'\_ and then _A_ will produce **á**.

Secondly, it's impossible to know which keys have been pressed just by looking at text. This is because the same character can be produced using different keys or key combinations (or it can be pasted, so the keyboard wouldn't be used at all).

So in order to just know how to transform the text you've mistyped, the app would have to keep track of the keys you've pressed, and then infer which of those pressed keys actually played a role in producing the mistyped text, and then ask the OS which characters would be produced with the same key sequence. You can see how that's unreasonable. First of all, the app would have to track _all_ the keys you press, and that would be quite sketchy. Would you trust an app which logs everything you type? I certainly wouldn't. And ethics aside, there still are technical issues. How many keys should the app track? What happens if the user pastes part of the text instead of typing it? Etc., etc.

And that's why the app uses a much simpler solution - it lets you, the user, figure out how the text should be transformed. The app can auto-configure the mappings using a simple method, but it's still up to you to check whether it's all correct.

## How Character Mappings Work

Here's how the tab looks when the app is opened for the first time:

![](../.gitbook/assets/v4.1-screen-char-mappings-empty.png)

You have to enter every character you can think of (which can be entered using your keyboard) into the text fields which correspond to layouts. For example, press the _Q_ key, then press the _W_ key, and so on. Then press _Shift+Q_, then _Shift+W_ etc. to add uppercase letters.

You should press the keys _in the same order_ for all layouts. Order matters here because the service will look at characters in same positions to determine how to transform the text. For example, let's say you have two layouts in your system: English (US) and Ukrainian (Enhanced). You put **q** as the first character in the text field for English, and **й** as the first character in the text field for Ukrainian (because that's the character you get when you press the _Q_ key). Now the service will know that when it switches from English to Ukrainian, it should transform **q** to **й**. If the second characters are **w** and **ц** for English and Ukrainian respectively, then the service will transform **w** to **ц**.

If you don't want to map a certain character, you can put a space in its position in the other layout. The space character acts as a _don't map this character_ command, though it should generally be avoided.

## Auto-Configuration

Even though you have to do it only once, configuring character mappings is still cumbersome and error-prone. That's why the app includes the auto-configuration feature. It's available only when all text fields are empty. The auto-configuration is quite simple (naïve even), and it shouldn't be 100% trusted, so you still have to check its output and fix mistakes, or add other characters manually. But usually auto-configuration should be good enough.

Here's how the tab looks after running auto-configuration:

![](../.gitbook/assets/v4.1-screen-auto-configuration.png)

### Windows and macOS

What auto-configuration does on Windows and macOS is basically ask the OS what would happen if certain keys were pressed using various layouts. There is no easy way to get the information about _all_ keys on your keyboard, so it asks only about the most common ones. Here's the list of those keys for the US layout:

* All letter keys (_Q_, _W_, _E_ etc.)
* Number keys
* Other common keys: _\[_ _]_ _;_ _'_ _,_ _._ _/_ _\\_ _-_ _=_ _\`_
* All those keys with the _Shift_ key
* All those keys with the _AltGr_/_Option_ key
* All those keys with the _Shift+AltGr_/_Option_ keys

Not all of those key combinations actually produce characters for all layouts. So after asking the OS about all of these keys, the app keeps only those character combinations which are defined for all layouts. For example, on Windows if you press _AltGr+U_ in the Ukrainian layout, you will get **ґ**, but if you press those keys in the US layout, you won't get anything at all, so the app will just throw **ґ** out as it won't know how to map it to the US layout.

### Linux

Unlike Windows and macOS, there's actually a way to ask the OS (though X11) which characters are produced by all possible key presses. What the app will do is ask about characters produced by all key codes on all shift levels (there are 64 of them!). As with Windows, it will keep only those combinations which are defined for all layouts.

## Limitations

You can map every character only once per layout. Otherwise, it would be impossible to map characters deterministically.

Characters are always mapped one to one. You cannot map one character to several characters.

Dead keys are not supported.

The space character cannot be mapped to other characters. This is not really a limitation, because the space character is the same in every layout (at least that's the assumption).

The app doesn't play nicely with text if you switched the layout yourself in the middle of typing. If that's the case, the app will not switch it correctly, and you will have to fix some of it yourself.
