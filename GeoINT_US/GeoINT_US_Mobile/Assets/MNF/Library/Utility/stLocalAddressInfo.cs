using System.Net;

namespace MNF
{

    public static partial class Utility
    {
        public struct stLocalAddressInfo
        {
            public IPAddress localAddress;
            public IPAddress subnetAddress;

            public stLocalAddressInfo(IPAddress localAddress, IPAddress subnetAddress)
            {
                this.localAddress = localAddress;
                this.subnetAddress = subnetAddress;
            }
        }
    }

    //      // don't use this function
    //// because this function will be crash in unity3d
    //      public static string getLocalIPAddress()
    //      {
    //	var host = Dns.GetHostEntry(Dns.GetHostName());
    //          foreach (var ip in host.AddressList)
    //          {
    //              if (ip.AddressFamily == AddressFamily.InterNetwork)
    //              {
    //                  return ip.ToString();
    //              }
    //          }
    //	return "127.0.0.1";
    //      }

    //static T GetByName<T>(object target, string methodName)
    //{
    //    MethodInfo method = target.GetType().GetMethod(methodName,
    //                   BindingFlags.Public
    //                   | BindingFlags.Instance
    //                   | BindingFlags.FlattenHierarchy);

    //    // Insert appropriate check for method == null here

    //    return (T)Delegate.CreateDelegate(typeof(T), target, method);
    //}

    //static T GetByName(object target, string methodName)
    //{
    //    return (T)Delegate.CreateDelegate
    //        (typeof(T), target, methodName);
    //}
    //public class TestLoadEnumsInClass
    //{
    //    public class TestingEnums
    //    {
    //        public enum Color { Red, Blue, Yellow, Pink }

    //        public enum Styles { Plaid = 0, Striped = 23, Tartan = 65, Corduroy = 78 }

    //        public string TestingProperty { get; set; }

    //        public string TestingMethod()
    //        {
    //            return null;
    //        }
    //    }

    //    public void btnTest_Click(object sender, EventArgs e)
    //    {
    //        var t = typeof(TestingEnums);
    //        var nestedTypes = t.GetMembers().Where(item => item.MemberType == MemberTypes.NestedType);
    //        foreach (var item in nestedTypes)
    //        {
    //            var type = Type.GetType(item.ToString());
    //            if (type == null || type.IsEnum == false)
    //                continue;

    //            string items = " ";
    //            foreach (MemberInfo x in type.GetMembers())
    //            {
    //                if (x.MemberType != MemberTypes.Field)
    //                    continue;

    //                if (x.Name.Equals("value__") == true)
    //                    continue;

    //                items = items + (" " + Enum.Parse(type, x.Name));
    //                items = items + (" " + (int)Enum.Parse(type, x.Name));
    //            }
    //            Console.WriteLine(items);
    //        }
    //    }
    //}

}
