namespace KeyboardSwitch.Linux.Xkb;

[Flags]
internal enum XkbContextFlags
{
    NoFlags = 0,
    NoDefaultIncludes = 1 << 0,
    NoEnvironmentNames = 1 << 1,
    NoSecureGetEnv = 1 << 2
}
