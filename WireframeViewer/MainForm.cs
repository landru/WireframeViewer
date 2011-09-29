using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WireframeViewer
{
    public partial class mainForm : Form
    {
        private Object3D wireframe;
        private Rectangle viewPort;
        const int viewportBorder = 4;
        // TO DO: read next two from theme properties at runtime
        const int formBorderWidth = 26; 
        const int formBorderHeight = 40;

        public mainForm()
        {
            InitializeComponent();
            UpdateTrackbarLabels();
            wireframe = new Object3D(-2.0, 2.0, -2.0, 2.0, -2.0, 2.0,
                trackBarZoom.Minimum, trackBarZoom.Maximum);
            
            // create a pyramid with a square base
            wireframe.AddVertex(1.0, -1.0, 1.0);
            wireframe.AddVertex(1.0, -1.0, -1.0);
            wireframe.AddVertex(-1.0, -1.0, -1.0);
            wireframe.AddVertex(-1.0, -1.0, 1.0);
            // the apex
            wireframe.AddVertex(0.0, 1.0, 0.0);
            
            // square base of pyramid
            wireframe.AddEdge(0, 1);
            wireframe.AddEdge(1, 2);
            wireframe.AddEdge(2, 3);
            wireframe.AddEdge(3, 0);
            // sides
            wireframe.AddEdge(4, 0);
            wireframe.AddEdge(4, 1);
            wireframe.AddEdge(4, 2);
            wireframe.AddEdge(4, 3);
        }

        private void mainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = Pens.CadetBlue;
            g.DrawRectangle(pen, viewPort);
            wireframe.Draw(g, viewPort, trackBarX.Value, trackBarY.Value,
                trackBarZ.Value, trackBarZoom.Value);
        }

        private void mainForm_Resize(object sender, EventArgs e)
        {
            AdjustViewport();
            Refresh();
        }

        private void AdjustViewport()
        {
            viewPort = new Rectangle(controlPanel.Right + viewportBorder, viewportBorder,
                (this.Width - (controlPanel.Width + formBorderWidth)), 
                this.Height - (2 * viewportBorder + formBorderHeight));
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            AdjustViewport();
        }

        private void UpdateTrackbarLabels()
        {
            labelX.Text = "X  " + trackBarX.Value.ToString();
            labelY.Text = "Y  " + trackBarY.Value.ToString();
            labelZ.Text = "Z  " + trackBarZ.Value.ToString();
            lblZoom.Text = "zoom  " + trackBarZoom.Value.ToString();
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            UpdateTrackbarLabels();
            Refresh(); // repaint
        }
    }
}
