using System;
using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Xam.Models;
using DidacticalEnigma.Xam.Services;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Xam.Droid
{
    [Activity(Label = "DidacticalEnigma.Xam", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            /*ServiceLocator.DownloadData(Xamarin.Essentials.FileSystem.AppDataDirectory);*/
            ServiceLocator.Configure(
                GetExternalFilesDir(null).AbsolutePath,
                Xamarin.Essentials.FileSystem.CacheDirectory);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}