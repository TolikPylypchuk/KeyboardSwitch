// The 'Switch Layout' extension for GNOME, used by Keyboard Switch to switch layouts

import Gio from 'gi://Gio';
import { Extension } from 'resource:///org/gnome/shell/extensions/extension.js';
import { getInputSourceManager } from 'resource:///org/gnome/shell/ui/status/keyboard.js';

const DBUS_INTERFACE = `
<node>
    <interface name="org.gnome.Shell.Extensions.SwitchLayout">
        <method name="Call">
            <arg type="u" direction="in" name="group" />
        </method>
    </interface>
</node>`;

export default class SwitchLayoutExtension extends Extension {

    enable() {
        this._dbus = Gio.DBusExportedObject.wrapJSObject(DBUS_INTERFACE, this);
        this._dbus.export(Gio.DBus.session, '/org/gnome/Shell/Extensions/SwitchLayout');
    }

    disable() {
        this._dbus.flush();
        this._dbus.unexport();
        delete this._dbus;
    }

    Call(group) {
        getInputSourceManager().inputSources[group].activate();
    }
}
