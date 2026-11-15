#if NEWER
[assembly: System.Reflection.AssemblyFileVersion("99.0.0.0")]
#elif OLDER
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]
#else
[assembly: System.Reflection.AssemblyFileVersion("2.6.0.0")]
#endif
internal static class TestStub
{
    private static int Main(string[] args)
    {
#if RESET_FAIL
        return args.Length > 0 && args[0] == "--reset-gamma" ? 1 : 0;
#else
        return 0;
#endif
    }
}
