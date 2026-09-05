// The 'Switch Layout' extension for GNOME, used by Keyboard Switch to switch layouts

const { Gio } = imports.gi;

const DBUS_INTERFACE = `
<node>
    <interface name="org.gnome.Shell.Extensions.SwitchLayout">
        <method name="SetCurrentLayout">
            <arg type="u" direction="in" name="index" />
        </method>
        <method name="GetCurrentLayout">
            <arg type="u" direction="out" name="index" />
            <arg type="s" direction="out" name="xkbId" />
            <arg type="s" direction="out" name="displayName" />
            <arg type="s" direction="out" name="shortName" />
        </method>
        <method name="GetLayouts">
            <arg type="a(usss)" direction="out" name="layouts" />
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

    GetCurrentLayout() {
        const source = imports.ui.status.keyboard.getInputSourceManager().currentSource;

        if (!source) {
            throw new Error('There is no current input source');
        }

        return [source.index, source.xkbId, source.displayName, source.shortName];
    }

    GetLayouts() {
        const sources = imports.ui.status.keyboard.getInputSourceManager().inputSources;

        return Object.keys(sources)
            .map(key => sources[key])
            .sort((left, right) => left.index - right.index)
            .map(source => [source.index, source.xkbId, source.displayName, source.shortName]);
    }

    SetCurrentLayout(index) {
        imports.ui.status.keyboard.getInputSourceManager().inputSources[index].activate();
    }
}

function init() {
    return new Extension();
}
