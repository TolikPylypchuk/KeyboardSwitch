// The 'Switch Layout' extension for GNOME, used by Keyboard Switch to switch layouts

const { Gio } = imports.gi;

const DBUS_INTERFACE = `
<node>
    <interface name="org.gnome.Shell.Extensions.SwitchLayout">
        <method name="Call">
            <arg type="u" direction="in" name="group" />
        </method>
    </interface>
</node>`;

class Extension {

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
        imports.ui.status.keyboard.getInputSourceManager().inputSources[group].activate();
    }
}

function init() {
    return new Extension();
}
