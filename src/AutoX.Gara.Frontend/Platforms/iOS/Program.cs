using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using UIKit;
namespace AutoX.Gara.Frontend;
public static class Program
{
    // This is the main entry point of the application.
    // if you want to use a different Application Delegate class from "AppDelegate"
    // you can specify it here.
    public static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
}
