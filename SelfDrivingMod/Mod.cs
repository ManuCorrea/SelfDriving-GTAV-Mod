using GTA;
using System;
using System.Windows.Forms;
using GTA.Math;
using System.IO;
using GTA.Native;
using System.Drawing;
using System.Drawing.Imaging;

public class ScriptTutorial : Script
{
    //Player info
    Ped playerPed;
    Vehicle vehicle;

    //Camera
    Camera cam;
    Vector3 rot;

    //Map data
    public Vector3 coordinates;
    public GTA.Math.Vector3 offset;
    public GTA.Math.Vector3 mapDestination;
    OutputArgument direction = new OutputArgument(), vehicleOut = new OutputArgument(), distance = new OutputArgument();
    
    int camAngle = 0;
    Rectangle bounds = new Rectangle(0, 0, 800, 480);
    Bitmap bitmap;
    Graphics g;

    //Front Image variables
    int width;
    int height;
    Size size;
    Size size2;
    Rectangle bounds2 = new Rectangle(0, 0, 600, 300);

    //Left Image variables
    Bitmap bitmap2;
    Graphics g2;
    int width2;
    int height2;

    Boolean onRoad = false;
    float speed;
    float heading;
    string json;

    //Interval
    byte imageInterval = 0;
    readonly byte endInterval = 3;

    //Top left point in the rectangle that take the screenshot
    Point p1 = new Point(0, 150);
    Point p2 = new Point(0, 250);

    bool simulate_cameras = false;
    string path;
    string image_front_path;
    string image_left_path;
    string json_path;


    public ScriptTutorial()
    {
        Tick += OnTick;
        KeyUp += OnKeyUp;

        World.DestroyAllCameras();
        cam = World.CreateCamera(new Vector3(1956.543f, 3699.894f, 35), Vector3.Zero, 60);
        cam.IsActive = true;

        width = bounds.Width;
        height = bounds.Height;

        width2 = bounds2.Width;
        height2 = bounds2.Height;
        
        size = bounds.Size;
        size2 = bounds2.Size;
        bitmap = new Bitmap(width, height);
        bitmap2 = new Bitmap(width2, height2);
        g = Graphics.FromImage(bitmap); //Screenshot front car
        g2 = Graphics.FromImage(bitmap2); //Screenshot left car

        string directory = Directory.GetCurrentDirectory();
        string combined = Path.Combine(directory, "scripts");

        string texto = Path.Combine(combined, Path.GetFileName("path.txt"));

        path = System.IO.File.ReadAllText(@texto);
        image_front_path = Path.Combine(path, Path.GetFileName("front.jpg"));
        image_left_path = Path.Combine(path, Path.GetFileName("left.jpg"));
        json_path = Path.Combine(path, Path.GetFileName("data_file.json"));

    }

    void OnTick(object sender, EventArgs e)
    {
        if (simulate_cameras == true)
        {
            if (imageInterval == endInterval)
            {
                if (camAngle == 60)
                {
                    camAngle = 0;
                }
                else
                {
                    camAngle += 60;
                }
                imageInterval = 0;
            }
        }
        else
        {
            imageInterval = 2;
        }


        playerPed = Game.Player.Character;
        

        if (playerPed.IsInVehicle())
        {
            GetGameData();
            ManageGameCamera();

            try
            {
                //I know those if statements looks inefficient, if I make them "properly" it goes fast enough to capture the camAngle they aren't suppose
                //to capture(at least in my PC). Just try changging the accepted imageInterval to see what is the best for you.
                if (!(imageInterval == 0 || imageInterval == 1 || imageInterval == endInterval))
                {
                    if (camAngle == 0)
                    {
                        g.CopyFromScreen(p1, Point.Empty, size);
                        bitmap.Save(image_front_path, ImageFormat.Jpeg);
                    }
                    else if (camAngle == 60)
                    {
                        g2.CopyFromScreen(p2, Point.Empty, size2);
                        bitmap2.Save(image_left_path, ImageFormat.Jpeg);
                    }
                }
                imageInterval += 1;
            }
            
            catch
            {
                //It doesn't matters if previously safe fails.
            }

            try
            {
                json = File.ReadAllText(json_path);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                jsonObj["car"]["speed"] = speed;
                jsonObj["car"]["incar"] = true;
                jsonObj["car"]["coordsX"] = coordinates.X;
                jsonObj["car"]["coordsY"] = coordinates.Y;
                jsonObj["car"]["coordsZ"] = coordinates.Z;
                jsonObj["car"]["heading"] = rot.Z;
                jsonObj["car"]["direction"] = direction.GetResult<int>();
                jsonObj["car"]["on_road"] = onRoad;
                jsonObj["car"]["2cam"] = simulate_cameras;
                jsonObj["car"]["destinationX"] = mapDestination.X;
                jsonObj["car"]["destinationY"] = mapDestination.Y;
                jsonObj["car"]["destinationZ"] = mapDestination.Z;

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(json_path, output);
            }
            catch
            {
            }          

        }
        else
        {
            OnFootCamera();
                                              
            json = File.ReadAllText(path);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            jsonObj["car"]["incar"] = false;  

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, output);
            }
    }

    public void MountCameraOnVehicle()
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
            MountCameraOnVehicle();
        }
        if (e.KeyCode == Keys.X)
        {
            if (simulate_cameras == true){
                simulate_cameras = false;
                camAngle = 0;
            }
            else
            {
                simulate_cameras = true;
            }
        }
    }

    void OnFootCamera()
    {
        World.DestroyAllCameras();
        Function.Call(Hash.DETACH_CAM, cam);
        Function.Call(Hash.RENDER_SCRIPT_CAMS, 0, 0, cam.Handle, 0, 0);
        World.RenderingCamera = cam;
        cam.AttachTo(playerPed, playerPed.Position);
        rot = playerPed.Rotation;
        Function.Call(Hash.SET_CAM_ROT, cam, rot.X, rot.Y, rot.Z, 2);
        Function.Call(Hash.GET_RENDERING_CAM, cam);
    }

    void GetGameData()
    {
        vehicle = playerPed.CurrentVehicle;
        //GPS & Coords
        mapDestination = World.GetWaypointPosition(); //https://github.com/Ric4o/GTAV-EnhancedNativeTrainer/blob/master/EnhancedNativeTrainer/src/features/teleportation.cpp
        mapDestination.Z = World.GetGroundHeight(mapDestination);
        Function.Call(Hash.GENERATE_DIRECTIONS_TO_COORD, mapDestination.X, mapDestination.Y, mapDestination.Z, 1, direction, vehicleOut, distance);

        speed = vehicle.Speed;
        heading = vehicle.Heading;
        rot = vehicle.Rotation;
        coordinates = vehicle.Position;

        onRoad = Function.Call<bool>(Hash.IS_POINT_ON_ROAD, coordinates.X, coordinates.Y, coordinates.Z, vehicle);

    }

    void ManageGameCamera()
    {
        World.DestroyAllCameras();
        cam = World.CreateCamera(new Vector3(1956.543f, 3699.894f, 35), Vector3.Zero, 60);
        cam.IsActive = true;

        cam.AttachTo(vehicle, new Vector3(0f, 2f, 1.2f));

        Function.Call(Hash.SET_CAM_ROT, cam, rot.X, rot.Y, rot.Z + camAngle, 2);
        GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, cam.Handle, true, true);
    }
}