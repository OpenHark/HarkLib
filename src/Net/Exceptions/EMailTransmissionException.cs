namespace HarkLib.Net
{
    [System.Serializable]
    public class EMailTransmissionException : System.Exception
    {
        public EMailTransmissionException(int code, string step)
            : base("MTA returned " + code + " in step " + step + ".")
        { }
    }
}