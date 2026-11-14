#if NEWER
[assembly: System.Reflection.AssemblyFileVersion("99.0.0.0")]
#elif OLDER
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]
#else
[assembly: System.Reflection.AssemblyFileVersion("2.6.0.0")]
#endif
internal static class TestStub
{
    private static int Main(string[] args) { return 0; }
}
