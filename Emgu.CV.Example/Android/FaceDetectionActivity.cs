﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Preferences;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using FaceDetection;

namespace AndroidExamples
{
   [Activity(Label = "Face Detection")]
   public class FaceDetectionActivity : ButtonMessageImageActivity
   {
      public FaceDetectionActivity()
         : base("Detect Face")
      {
      }

      protected override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);

         OnButtonClick += delegate
         {
            using (Image<Bgr, Byte> image = PickImage("lena.jpg"))
            {
               ISharedPreferences preference = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
               String appVersion = PackageManager.GetPackageInfo(PackageName, Android.Content.PM.PackageInfoFlags.Activities).VersionName;
               if (!preference.Contains("cascade-data-version") || !preference.GetString("cascade-data-version", null).Equals(appVersion)
                  || !(preference.Contains("cascade-eye-data-path") || preference.Contains("cascade-face-data-path")))
               {
                  AndroidFileAsset.OverwriteMethod overwriteMethod = AndroidFileAsset.OverwriteMethod.AlwaysOverwrite;

                  FileInfo eyeFile = AndroidFileAsset.WritePermanantFileAsset(this, "haarcascade_eye.xml", "cascade", overwriteMethod);
                  FileInfo faceFile = AndroidFileAsset.WritePermanantFileAsset(this, "haarcascade_frontalface_default.xml", "cascade", overwriteMethod);

                  //save tesseract data path
                  ISharedPreferencesEditor editor = preference.Edit();
                  editor.PutString("cascade-data-version", appVersion);
                  editor.PutString("cascade-eye-data-path", eyeFile.FullName);
                  editor.PutString("cascade-face-data-path", faceFile.FullName);
                  editor.Commit();
               }

               string eyeXml = preference.GetString("cascade-eye-data-path", null);
               string faceXml = preference.GetString("cascade-face-data-path", null);
               long time;
               List<Rectangle> faces = new List<Rectangle>();
               List<Rectangle> eyes = new List<Rectangle>();
               DetectFace.Detect(image, faceXml, eyeXml, faces, eyes, out time);
               SetMessage(String.Format("Detected in {0} milliseconds.", time));

               Bitmap bmp = image.ToBitmap();
               using (Canvas c = new Canvas(bmp))
               using (Paint p = new Paint())
               {
                  p.Color = Android.Graphics.Color.Red;
                  p.StrokeWidth = 2;
                  p.SetStyle(Paint.Style.Stroke);
                  foreach (Rectangle rect in faces)
                     c.DrawRect(new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom), p);

                  p.Color = Android.Graphics.Color.Blue;
                  foreach (Rectangle rect in eyes)
                     c.DrawRect(new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom), p);
               }

               SetImageBitmap(bmp);
            }
         };
      }
   }
}

