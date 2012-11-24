using System;
using UnityEngine;
using KSP.IO;
using System.Net;
using System.Text;
using RedCorona.Net;

public class Telemachus : PartModule
{
    public const string UriAddress = "http://localhost:8888/";

    public override void OnLoad(
      ConfigNode node)
    {
        UnityEngine.Debug.Log("-------------onload-----debug-----------------------");
        HttpServer http = new HttpServer(new Server(8888));
        http.Handlers.Add(new SubstitutingFileReader());
    }
}

