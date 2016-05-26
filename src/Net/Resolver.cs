using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Net;
using System;

namespace HarkLib.Net
{
    public static class Resolver
    {
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms682016(v=vs.85).aspx
        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsQuery(
            [MarshalAs(UnmanagedType.VBByRefStr)]
            ref string pszName,
            QueryTypes wType,
            QueryOptions options,
            int aipServers,
            ref IntPtr ppQueryResults,
            int pReserved
        );

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms682021(v=vs.85).aspx
        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);

        public static IPAddress[] GetIPs(string domain)
        {
            return Dns.GetHostEntry(domain).AddressList;
        }
        
        public static string[] GetMxRecords(string domain)
        {
            return GetRecords(domain, QueryTypes.DNS_TYPE_MX);
        }
        
        [HandleProcessCorruptedStateExceptionsAttribute()]
        public static string[] GetRecords(string domain, QueryTypes type = QueryTypes.DNS_TYPE_ALL, QueryOptions options = QueryOptions.DNS_QUERY_BYPASS_CACHE)
        {
            if(Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new NotSupportedException(); // Can't use 'DnsQuery'
            
            IntPtr entriesStartPtr = IntPtr.Zero;
            
            int status = DnsQuery(ref domain, type, options, 0, ref entriesStartPtr, 0);
            if(status != 0)
                throw new Exception("Error by 'DnsQuery' : " + status);
            
            List<string> results = new List<string>();
            Record record;
            
            for(IntPtr ptr = entriesStartPtr; !ptr.Equals(IntPtr.Zero); ptr = record.pNext)
            {
                record = (Record)Marshal.PtrToStructure(ptr, typeof(Record));
                
                if(record.wType == (short)QueryTypes.DNS_TYPE_A || record.wType == (short)QueryTypes.DNS_TYPE_AAAA)
                    continue;
                
                try
                {
                    if((record.wType & (short)type) != 0)
                        results.Add(Marshal.PtrToStringAuto(record.pNameExchange));
                }
                catch(System.AccessViolationException)
                { }
            }
            
            DnsRecordListFree(entriesStartPtr, 0);
            return results.ToArray();
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/cc982162(v=vs.85).aspx
        public enum QueryOptions
        {
            DNS_QUERY_STANDARD                  = 0x00000000,
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 0x00000001,
            DNS_QUERY_USE_TCP_ONLY              = 0x00000002,
            DNS_QUERY_NO_RECURSION              = 0x00000004,
            DNS_QUERY_BYPASS_CACHE              = 0x00000008,
            DNS_QUERY_NO_WIRE_QUERY             = 0x00000010,
            DNS_QUERY_NO_LOCAL_NAME             = 0x00000020,
            DNS_QUERY_NO_HOSTS_FILE             = 0x00000040,
            DNS_QUERY_NO_NETBT                  = 0x00000080,
            DNS_QUERY_WIRE_ONLY                 = 0x00000100,
            DNS_QUERY_RETURN_MESSAGE            = 0x00000200,
            DNS_QUERY_MULTICAST_ONLY            = 0x00000400,
            DNS_QUERY_NO_MULTICAST              = 0x00000800,
            DNS_QUERY_TREAT_AS_FQDN             = 0x00001000,
            DNS_QUERY_ADDRCONFIG                = 0x00002000,
            DNS_QUERY_DUAL_ADDR                 = 0x00004000,
            DNS_QUERY_MULTICAST_WAIT            = 0x00020000,
            DNS_QUERY_MULTICAST_VERIFY          = 0x00040000,
            DNS_QUERY_DONT_RESET_TTL_VALUES     = 0x00100000,
            DNS_QUERY_DISABLE_IDN_ENCODING      = 0x00200000,
            DNS_QUERY_APPEND_MULTILABEL         = 0x00800000
        }

        public enum QueryTypes
        {
            DNS_TYPE_A          = 0x0001,
            DNS_TYPE_NS         = 0x0002,
            DNS_TYPE_MD         = 0x0003,
            DNS_TYPE_MF         = 0x0004,
            DNS_TYPE_CNAME      = 0x0005,
            DNS_TYPE_SOA        = 0x0006,
            DNS_TYPE_MB         = 0x0007,
            DNS_TYPE_MG         = 0x0008,
            DNS_TYPE_MR         = 0x0009,
            DNS_TYPE_NULL       = 0x000a,
            DNS_TYPE_WKS        = 0x000b,
            DNS_TYPE_PTR        = 0x000c,
            DNS_TYPE_HINFO      = 0x000d,
            DNS_TYPE_MINFO      = 0x000e,
            DNS_TYPE_MX         = 0x000f,
            DNS_TYPE_TEXT       = 0x0010,
            DNS_TYPE_RP         = 0x0011,
            DNS_TYPE_AFSDB      = 0x0012,
            DNS_TYPE_X25        = 0x0013,
            DNS_TYPE_ISDN       = 0x0014,
            DNS_TYPE_RT         = 0x0015,
            DNS_TYPE_NSAP       = 0x0016,
            DNS_TYPE_NSAPPTR    = 0x0017,
            DNS_TYPE_SIG        = 0x0018,
            DNS_TYPE_KEY        = 0x0019,
            DNS_TYPE_PX         = 0x001a,
            DNS_TYPE_GPOS       = 0x001b,
            DNS_TYPE_AAAA       = 0x001c,
            DNS_TYPE_LOC        = 0x001d,
            DNS_TYPE_NXT        = 0x001e,
            DNS_TYPE_EID        = 0x001f,
            DNS_TYPE_NIMLOC     = 0x0020,
            DNS_TYPE_SRV        = 0x0021,
            DNS_TYPE_ATMA       = 0x0022,
            DNS_TYPE_NAPTR      = 0x0023,
            DNS_TYPE_KX         = 0x0024,
            DNS_TYPE_CERT       = 0x0025,
            DNS_TYPE_A6         = 0x0026,
            DNS_TYPE_DNAME      = 0x0027,
            DNS_TYPE_SINK       = 0x0028,
            DNS_TYPE_OPT        = 0x0029,
            DNS_TYPE_DS         = 0x002B,
            DNS_TYPE_RRSIG      = 0x002E,
            DNS_TYPE_NSEC       = 0x002F,
            DNS_TYPE_DNSKEY     = 0x0030,
            DNS_TYPE_DHCID      = 0x0031,
            DNS_TYPE_UINFO      = 0x0064,
            DNS_TYPE_UID        = 0x0065,
            DNS_TYPE_GID        = 0x0066,
            DNS_TYPE_UNSPEC     = 0x0067,
            DNS_TYPE_ADDRS      = 0x00f8,
            DNS_TYPE_TKEY       = 0x00f9,
            DNS_TYPE_TSIG       = 0x00fa,
            DNS_TYPE_IXFR       = 0x00fb,
            DNS_TYPE_AXFR       = 0x00fc,
            DNS_TYPE_MAILB      = 0x00fd,
            DNS_TYPE_MAILA      = 0x00fe,
            DNS_TYPE_ALL        = 0x00ff,
            DNS_TYPE_ANY        = 0x00ff,
            DNS_TYPE_WINS       = 0xff01,
            DNS_TYPE_WINSR      = 0xff02,
            DNS_TYPE_NBSTAT     = DNS_TYPE_WINSR
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms682082(v=vs.85).aspx
        [StructLayout(LayoutKind.Sequential)]
        private struct Record
        {
            public IntPtr pNext;
            public string pName;
            public short wType;
            public short wDataLength;
            public int Flags;
            public int dwTtl;
            public int dwReserved;
            public IntPtr pNameExchange;
            public short wPreference;
            public short Pad;
        }
    }
}