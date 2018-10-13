using GTA;
using System;
using System.Windows.Forms;
using GTA.Math;
using System.IO;
using GTA.Native;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class ScriptTutorial : Script
{
    Camera cam;
    Vector3 rot;
    public Vector3 coordinates;
    public GTA.Math.Vector3 offset;
    public GTA.Math.Vector3 mapDestination;
    OutputArgument direction = new OutputArgument(), vehicleOut = new OutputArgument(), distance = new OutputArgument();
    Ped playerPed;
    int variable = 60;
    Rectangle bounds = new Rectangle(0, 0, 800, 600);
    Bitmap bitmap;
    Graphics g;

    int width;
    int height;
    Size size;

    string _path;
    string text = "pos no";

    Vector3 coords;
    Boolean onRoad = false;
    float velo;
    string speed;
    float heading;
    string json;

    byte magicNumber = 0;

    string path = @"c:\Users\yo\Desktop\SelfDriving\joystickrecolDataTraining\data_file.json";

    Point p1 = new Point();
    long compare_time;
    long save_time;

    Bitmap prev_front;



    public ScriptTutorial()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;

        Interval = 1;

        World.DestroyAllCameras();
        cam = World.CreateCamera(new Vector3(1956.543f, 3699.894f, 35), Vector3.Zero, 60);
        cam.IsActive = true;

        width = bounds.Width;
        height = bounds.Height;
        size = bounds.Size;
        bitmap = new Bitmap(width, height);
        g = Graphics.FromImage(bitmap);
        p1.X = 0;
        p1.Y = 30;

               
        //GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, cam.Handle, true, true);

    }

    void OnTick(object sender, EventArgs e)
    {
        UI.ShowSubtitle("loaded");
        if (magicNumber == 5)
        {
            if (variable == 60)
            {
                variable = 0;

            }
            else
            {
                variable += 60;

            }
            magicNumber = 0;
        }

        playerPed = Game.Player.Character;
        Vehicle vehicle = playerPed.CurrentVehicle;


        if (playerPed.IsInVehicle())
        {
            //GPS & Coords
            mapDestination = World.GetWaypointPosition(); //https://github.com/Ric4o/GTAV-EnhancedNativeTrainer/blob/master/EnhancedNativeTrainer/src/features/teleportation.cpp
            mapDestination.Z = World.GetGroundHeight(mapDestination);
            Function.Call(Hash.GENERATE_DIRECTIONS_TO_COORD, mapDestination.X, mapDestination.Y, mapDestination.Z, 1, direction, vehicleOut, distance);


            velo = vehicle.Speed;
            speed = velo.ToString();
            heading = vehicle.Heading;
            rot = vehicle.Rotation;
            coordinates = vehicle.Position;

            onRoad = Function.Call<bool>(Hash.IS_POINT_ON_ROAD, coordinates.X, coordinates.Y, coordinates.Z, vehicle);


            World.DestroyAllCameras();
            cam = World.CreateCamera(new Vector3(1956.543f, 3699.894f, 35), Vector3.Zero, 60);
            cam.IsActive = true;

            rot = vehicle.Rotation;

            cam.AttachTo(vehicle, new Vector3(0f, 2f, 1f));

            Function.Call(Hash.SET_CAM_ROT, cam, rot.X, rot.Y, rot.Z + variable, 2);
            GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, cam.Handle, true, true);

            try
            {
                if (variable == 0 && (magicNumber == 3 || magicNumber == 4 || magicNumber == 2))
                {

                    g.CopyFromScreen(p1, Point.Empty, size);
                    //C:\Users\yo\Desktop\SelfDriving\joystickrecolDataTraining

                    Stopwatch sw = new Stopwatch();

                    sw.Start();
                    bool iguales = CompareBitmapsFast(bitmap, prev_front);
                    sw.Stop();
                    compare_time = sw.ElapsedMilliseconds;
                    if (iguales == false)
                    {
                        sw.Restart();
                        bitmap.Save(@"c:\Users\yo\Desktop\SelfDriving\joystickrecolDataTraining\test.bmp", ImageFormat.Bmp);
                        prev_front = (Bitmap)bitmap.Clone();

                        sw.Stop();
                        save_time = sw.ElapsedMilliseconds;
                    }
                    else
                    {
                        save_time = 0;
                    }

                    

                    //Stopwatch para tiempo salvar y otro tiempo comparar

                }
                else if (variable == 60 && (magicNumber == 3 || magicNumber == 4 || magicNumber == 2))
                {
                    g.CopyFromScreen(p1, Point.Empty, size);
                    bitmap.Save(@"c:\Users\yo\Desktop\SelfDriving\joystickrecolDataTraining\tost.bmp", ImageFormat.Bmp);
                }

            }
            catch(Exception exeption)
            {
                UI.Notify(exeption.ToString());
                UI.Notify("error");
            }

            try
            {
                json = File.ReadAllText(path);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                jsonObj["car"]["speed"] = velo;
                jsonObj["car"]["incar"] = true;
                jsonObj["car"]["coordsX"] = coordinates.X.ToString();
                jsonObj["car"]["coordsY"] = coordinates.Y.ToString();
                jsonObj["car"]["coordsZ"] = coordinates.Z.ToString();
                jsonObj["car"]["heading"] = rot.Z.ToString();
                jsonObj["car"]["direction"] = direction.GetResult<int>().ToString();
                jsonObj["car"]["on_road"] = onRoad;
                jsonObj["car"]["path"] = text;
                jsonObj["car"]["time_compare"] = compare_time.ToString();
                jsonObj["car"]["time_save"] = save_time.ToString();


                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(path, output);
            }
            finally
            {

            }
            

            magicNumber += 1;

        }
        else
        {
            World.DestroyAllCameras();
            Function.Call(Hash.DETACH_CAM, cam);
            Function.Call(Hash.RENDER_SCRIPT_CAMS, 0, 0, cam.Handle, 0, 0);
            World.RenderingCamera = cam;
            cam.AttachTo(playerPed, playerPed.Position);
            rot = playerPed.Rotation;
            Function.Call(Hash.SET_CAM_ROT, cam, rot.X, rot.Y, rot.Z, 2);
            Function.Call(Hash.GET_RENDERING_CAM, cam);

            
            //UI.Notify("no en coche man");

            json = File.ReadAllText(path);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            jsonObj["car"]["incar"] = false;
            //Console.ReadKey();
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, output);

        }
    }

    void OnKeyDown(object sender, KeyEventArgs e)
    {

    }

    public void mountCameraOnVehicle()
    {
        if (Game.Player.Character.IsInVehicle())
        {
            GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, cam.Handle, true, true);
        }
        else
        {
            UI.Notify("Please enter a vehicle.");
        }
    }

    void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.I)
        {
            mountCameraOnVehicle();
        }
    }

    public static bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
    {
        int counter = 0;
        if (bmp1 == null || bmp2 == null)
            return false;

        if (object.Equals(bmp1, bmp2))
            return true;

        if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
            return false;

        int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

        bool result = true;
        byte[] b1bytes = new byte[bytes];
        byte[] b2bytes = new byte[bytes];

        BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width - 1, bmp1.Height - 1), ImageLockMode.ReadOnly, bmp1.PixelFormat);
        BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width - 1, bmp2.Height - 1), ImageLockMode.ReadOnly, bmp2.PixelFormat);

        Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
        Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

        for (int n = 0; n <= bytes - 1; n++)
        {
            if (b1bytes[n] != b2bytes[n])
            {
                result = false;
                break;
            }
        }

        bmp1.UnlockBits(bitmapData1);
        bmp2.UnlockBits(bitmapData2);

        return result;
    }

    public static bool CompareBitmapsLazy(Bitmap bmp1, Bitmap bmp2)
    {
        int counter = 0;
        if (bmp1 == null || bmp2 == null)
            return false;

        if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
            return false;

        //Compare bitmaps using GetPixel method
        for (int column = 0; column < bmp1.Width; column++)
        {
            for (int row = 0; row < bmp1.Height; row++)
            {
                if (!bmp1.GetPixel(column, row).Equals(bmp2.GetPixel(column, row)))
                    counter += 1;
                    if (counter > 1000)
                    {
                        return false;
                    }
                    
            }
        }

        return true;
    }

}