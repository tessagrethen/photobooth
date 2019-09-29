using System;
using System.IO;
using Gtk;

namespace Photobooth
{
    class MainWindow : Window
    {
        public const float TOP = 0;
        public const float BOTTOM = 1;
        public const float LEFT = 0;
        public const float RIGHT = 1;
        public const float MIDDLE = 0.5f;
        public const float FIXED = 0;
        public const float FILL = 1;
        CompositeModel _model;
        TransformTool _transformTool;
        ListView _listView;
        Canvas _canvas;
        public MainWindow(string title) : base(title)
        {
        
        // COMPOSITE MODEL
            _model = new CompositeModel();
            _model.AddCompositeChangedCallback(UpdateCanvas);
            _model.AddLayerChangedCallback(UpdateListView);
        
        // TRANSFORM TOOL
            _transformTool = new TransformTool();

        // MENUBAR
            // FILE
            MenuBar menuBar = new MenuBar();
            
            MenuItem file = new MenuItem("File");
            Menu fileMenu = new Menu();
            file.Submenu = fileMenu;

            MenuItem open = new MenuItem("Open");
            open.Activated += OpenImage;
            fileMenu.Append(open);              

            MenuItem save = new MenuItem("Save");
            save.Activated += SaveImage;
            fileMenu.Append(save);

            MenuItem quit = new MenuItem("Quit");
            quit.Activated += delegate { Application.Quit(); };
            fileMenu.Append(quit);                 

            menuBar.Append(file);
            
            // FILTER
            MenuItem filter = new MenuItem("Filter");
            Menu filterMenu = new Menu();
            filter.Submenu = filterMenu;

            MenuItem none = new MenuItem("None");
            none.Activated += NoneF;
            filterMenu.Append(none);              

            MenuItem grayscale = new MenuItem("Grayscale");
            grayscale.Activated += GrayscaleF;
            filterMenu.Append(grayscale);

            MenuItem lighten = new MenuItem("Lighten");
            lighten.Activated += LightenF;
            filterMenu.Append(lighten);              

            MenuItem darken = new MenuItem("Darken");
            darken.Activated += DarkenF;
            filterMenu.Append(darken);

            MenuItem jitter = new MenuItem("Jitter");
            jitter.Activated += JitterF;
            filterMenu.Append(jitter);            

            menuBar.Append(filter);


        // TOOLBAR
            Toolbar toolbar = new Toolbar();
            toolbar.ToolbarStyle = ToolbarStyle.Icons;

            // Transform Tool Buttons
            DirectoryInfo diOp = new DirectoryInfo(@"./ops");
            for (int i = 0; i < diOp.GetFiles().Length; i++)
            {
                var fi = diOp.GetFiles()[i];
                Image op = new Image("./ops/" + fi.Name);
                string label = fi.Name.Substring(0, fi.Name.Length - 4);
                ToolButton addOp = new ToolButton(op, label);
                addOp.Clicked += ChangeTransformMode;
                toolbar.Insert(addOp, i);
            }

            // Accessory Buttons
            DirectoryInfo diAcc = new DirectoryInfo(@"./accessories");
            for (int i = 0; i < diAcc.GetFiles().Length; i++)
            {
                var fi = diAcc.GetFiles()[i];
                Image accessory = new Image("./accessories/" + fi.Name);
                accessory.Pixbuf = accessory.Pixbuf.ScaleSimple(40, 40, Gdk.InterpType.Bilinear);
                string label = fi.Name.Substring(0, fi.Name.Length - 4);
                ToolButton addAccessory = new ToolButton(accessory, label);
                addAccessory.Clicked += AddAccessory;
                toolbar.Insert(addAccessory, i + 2);
            }
        
        // LISTVIEW
            _listView = new ListView("Layer");

        // DELETE LAYER
            Button deleteLayer = new Button("Delete Layer");
            deleteLayer.Clicked += DeleteAccessory;

        // CANVAS
            _model.LoadBaseImage("./photos/kitty4.jpg");
            _canvas = new Canvas(_model.CompositeImage);
            _canvas.ButtonPressEvent += ActivateTransformTool;
            _canvas.MotionNotifyEvent += TransformLayer;
            _canvas.ButtonReleaseEvent += DeactivateTransformTool;

        // DISPLAY SETUP
            bool isUniform = false;
            int margin = 5;

            VBox listViewAndDeleteLayer = new VBox(isUniform, margin);
            listViewAndDeleteLayer.Add(Align(_listView, TOP, LEFT, FIXED, FIXED));
            listViewAndDeleteLayer.Add(Align(deleteLayer, BOTTOM, LEFT, FILL, FILL));


            HBox assortedThings = new HBox(isUniform, margin); 
            assortedThings.Add(Align(listViewAndDeleteLayer, TOP, MIDDLE, FIXED, FIXED));
            assortedThings.Add(Align(_canvas, TOP, LEFT, FILL, FIXED));
            
            VBox vlayout = new VBox(isUniform, margin);
            vlayout.Add(Align(menuBar, TOP, LEFT, FILL, FILL));
            vlayout.Add(Align(toolbar,TOP, LEFT, FIXED, FIXED));
            vlayout.Add(Align(assortedThings,TOP, LEFT, FILL, FILL));
            
            Add(vlayout);
            BorderWidth = (uint) margin * 2;
            Resize(800, 600);
            ShowAll();
        }

        // CALLBACKS FOR FILTERS
        public void GrayscaleF(object grayscalefilter, EventArgs args)
        {
            _model.RunFilter("GrayscaleFilter");
        }
        public void NoneF(object grayscalefilter, EventArgs args)
        {
            _model.RunFilter("NoneFilter");
        }
        public void LightenF(object lightenfilter, EventArgs args)
        {
            _model.RunFilter("LightenFilter");
        }
        public void DarkenF(object darkenfilter, EventArgs args)
        {
            _model.RunFilter("DarkenFilter");
        }

        public void JitterF(object jitterfilter, EventArgs args)
        {
            _model.RunFilter("JitterFilter");
        }

        // CALLBACKS FOR FIRST PART
        public void AddAccessory(object layerButton, EventArgs args)
        {
            try
            {
                ToolButton btn = layerButton as ToolButton;
                Image img = new Image("./accessories/" + btn.Label + ".png");
                _model.AddLayer(img.Pixbuf, btn.Label);
                _listView.Selected = _model.NumLayers - 1;
            }
            catch (Exception e)
            {
                MessageDialog md =
                    new MessageDialog (this, 
                                        DialogFlags.DestroyWithParent,
                                        MessageType.Error, 
                                        ButtonsType.Close, "Scale size out of bounds!");
    
                int result = md.Run();
                md.Destroy();
            }  
        }

        public void OpenImage(object openMenuItem, EventArgs args)
        {
            Gtk.FileChooserDialog fc =
		        new Gtk.FileChooserDialog("Open File",
                                            this,
                                            FileChooserAction.Open,
                                            "Cancel", ResponseType.Cancel,
                                            "Open", ResponseType.Accept);

            if (fc.Run() == (int) ResponseType.Accept) 
            {
                if (!_model.LoadBaseImage(fc.Filename))
                {
                    MessageDialog md =
                        new MessageDialog (this, 
                                            DialogFlags.DestroyWithParent,
                                            MessageType.Error, 
                                            ButtonsType.Close, "Error loading file!");
        
                    int result = md.Run();
                    md.Destroy();
                }
            }
            
            fc.Destroy();
        }

        public void SaveImage(object saveMenuItem, EventArgs args)
        {
            Gtk.FileChooserDialog fc =
            new Gtk.FileChooserDialog("Save File As",
                                        null,
                                        Gtk.FileChooserAction.Save,
                                        "Cancel", ResponseType.Cancel,
                                        "Save", ResponseType.Accept);

            if (fc.Run() == (int) Gtk.ResponseType.Accept)
            {
                if (!_model.SaveCompositeImage(fc.Filename))
                {
                    MessageDialog md =
                        new MessageDialog (this, 
                                            DialogFlags.DestroyWithParent,
                                            MessageType.Error, 
                                            ButtonsType.Close, "Error saving file!");
        
                    int result = md.Run();
                    md.Destroy();
                } 
            }

            fc.Destroy();
        }

        public void DeleteAccessory(object deleteLayerButton, EventArgs args)
        {
            _model.DeleteLayer(_listView.Selected);
            if (_model.NumLayers > 0) _listView.Selected = 0;
            else _listView.Selected = -1;
        }

        public void ChangeTransformMode(object transformToolButton, EventArgs args)
        {
            ToolButton btn = transformToolButton as ToolButton;
            if (btn.Label.Equals("scale")) { _transformTool.mode = Mode.SCALE; }
            else _transformTool.mode = Mode.TRANSLATE;
        }

        public void ActivateTransformTool(object canvasClick, ButtonPressEventArgs args)
        {
            double mouseX = args.Event.X;
            double mouseY = args.Event.Y;
            _transformTool.Activate(mouseX, mouseY);
            _transformTool.DoWork(mouseX, mouseY, _listView.Selected, _model);
        }

        public void TransformLayer(object canvasDrag, MotionNotifyEventArgs args)
        {
            try
            {
                double mouseX = args.Event.X;
                double mouseY = args.Event.Y;
                _transformTool.DoWork(mouseX, mouseY, _listView.Selected, _model);
            }
            catch (Exception e)
            {
                MessageDialog md =
                    new MessageDialog (this, 
                                        DialogFlags.DestroyWithParent,
                                        MessageType.Error, 
                                        ButtonsType.Close, "Scale size out of bounds!");
    
                int result = md.Run();
                md.Destroy();
            }
        }

        public void DeactivateTransformTool(object canvasRelease, ButtonReleaseEventArgs args)
        {
            _transformTool.Deactivate();
            _transformTool.mode = Mode.TRANSLATE;
        }

        public void UpdateCanvas()
        {
            _canvas.SetImage(_model.CompositeImage);
        }

        public void UpdateListView()
        {
            _listView.Clear();
            for (int i = 0; i < _model.NumLayers; i++)
            {
                _listView.AddItem(_model.GetLayerName(i));
            }
        }

        Alignment Align(Widget widget, float xalign, float yalign, float xscale, float yscale)
        {
            Alignment alignment = new Alignment(xalign, yalign, xscale, yscale);
            alignment.Add(widget);
            return alignment;
        }
    }
}

