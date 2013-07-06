using System;
using MinimalHTTPServer;
using System.Reflection;
using System.Collections.Generic;
using System.Text;


namespace ServersTest
{
    public class Program
    {
        public static int Main(String[] args)
        {
           /* ServerConfiguration config = new ServerConfiguration();
            Server server = new Server(config);
            server.OnServerNotify += new Server.ServerNotify(serverOut);
            server.addHTTPResponsibility(new ElseResponsibility());
            server.addHTTPResponsibility(new TelemachusResponsibility());
            
            server.startServing();*/

            argumentsParse("alt=tp.value&lan=tp2.value2");

            Console.Read();
            //server.stopServing();
            return 0;
        }

        private static void serverOut(String message)
        {
            Console.WriteLine(message);
        }

        static DataLinks dl = new DataLinks();
            
        static Dictionary<string, ReflectiveArgumentData> abstractArgument =
            new Dictionary<string, ReflectiveArgumentData>();

        private static void argumentsParse(String args)
        {
            String[] argsSplit = args.Split('&');

            foreach (String arg in argsSplit)
            {
                argumentParse(arg);
            }
        }

        private static void argumentParse(String args)
        {
            String[] argsSplit = args.Split('=');
            ReflectiveArgumentData ad = null;
            abstractArgument.TryGetValue(argsSplit[1], out ad);
            
            if (ad == null)
            {
                ad = new ReflectiveArgumentData();
                ad.key = argsSplit[1];
                abstractArgument.Add(argsSplit[1], ad);
            }

            ad.variableName = argsSplit[0];
            ad.updateValue(dl);
            Console.WriteLine(ad.ToString());
        }
    }

    public class ReflectiveArgumentData
    {
        public String variableName { get; set; }
        public String key { get; set; }
        public Object value { get; set; }

        FieldInfo field = null;

        public void updateValue(DataLinks dl)
        {
            if (field == null)
            {
                reflectiveUpdate(dl);
            }
            else
            {
                value = field.GetValue(null);
            }
        }

        private void reflectiveUpdate(DataLinks dl)
        {
            String[] argsSplit = key.Split('.');
            Type type = dl.GetType();
            value = dl;

            foreach(String s in argsSplit)
            {
                field = type.GetField(s);
                value = field.GetValue(value);

                type = value.GetType();
            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("var ");
            sb.Append(variableName);
            sb.Append(" = ");
            sb.Append(value.ToString());
            sb.Append(";");

            return sb.ToString();
        }
    }



    public class TestProgram
    {
        public double value;
        public double value2;
        public TestProgram()
        {
            value = 1;
            value2 = 5;
        }

        public int ret()
        {
            return 4;
        }

        public void hello()
        {
            Console.WriteLine("Hello");
        }
    }
}

