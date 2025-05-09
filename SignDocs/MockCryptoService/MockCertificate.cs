namespace SignDocs
{
    public class MockCertificate
    {
        public string SerialNumber { get; private set; } = "1234567890";
        public string Policy { get; private set; } = "123.456.789.0";
        public string Usage { get; private set; } = "digitalSignature nonRepudiation";
        public string Name { get; private set; } = "Paul Harris";
        public string GivenName { get; private set; } = null;
    }
}
