using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Grapher
{
    public static class Graph
    {
        private const int margin = 100;
        public static Bitmap LineGraph(int graphHeight, int graphWidth, long[] values, string xAxis, string yAxis, Color colour)
        {

            var bmp = new Bitmap(graphWidth, graphHeight);
            var g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, graphWidth, graphHeight);
            var font = new Font("Times New Roman", 20);

            var stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            
            g.DrawString(yAxis, font, new SolidBrush(Color.Black),5, 20,stringFormat);
            
            var numberOfValues = values.Count();
            var step = ((decimal)graphWidth - margin) / numberOfValues;
            var minValue = values.Min();
            var maxValue = values.Max();

            font = new Font("Times New Roman", 10);
            stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Far;
            //g.DrawString(maxValue.ToString(), font, new SolidBrush(Color.Black), margin - (maxValue.ToString().Length * 8), 0);
            //g.DrawString(minValue.ToString(), font, new SolidBrush(Color.Black), margin - (minValue.ToString().Length * 8), graphHeight - margin-10);
            g.DrawString(maxValue.ToString(), font, new SolidBrush(Color.Black),new Rectangle(0,0,margin,15), stringFormat);// margin - (maxValue.ToString().Length * 8), 0);
            g.DrawString(minValue.ToString(), font, new SolidBrush(Color.Black), new Rectangle(0, graphHeight-margin-5, margin, 15), stringFormat);// margin - (minValue.ToString().Length * 8), graphHeight - margin - 10);

            var range = maxValue - minValue;
            g.DrawLine(new Pen(Color.Black), margin, 0, margin, graphHeight-margin);
            g.DrawLine(new Pen(Color.Black), margin, graphHeight-margin, graphWidth, graphHeight-margin);

            var heightMultiplier = ((decimal)graphHeight - margin) / range;
            var oldX = (decimal)margin;
            var oldY = graphHeight- margin- ((values[0]-minValue) * heightMultiplier);

            for (var i =0; i<values.Count(); i++)
            {
                var newX = i * step + margin;
                var newY = graphHeight - margin -((values[i]-minValue) * heightMultiplier);
                g.DrawLine(new Pen(colour), (float)oldX, (float)oldY, (float)newX, (float)newY);
                oldX = newX;
                oldY = newY;
            }
            return bmp;
        }
    }
}
