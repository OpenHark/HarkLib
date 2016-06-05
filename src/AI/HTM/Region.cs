using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

using HarkLib.Core;

namespace HarkLib.AI
{
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public class Region
    {
        public Region(int sizeX, int sizeY)
        {
            this.ActiveColumns = new List<Column>();
            this.Columns = new List<Column>();
            //this.Segment = DendriteSegment
            
            for(int x = 0; x < sizeX; ++x)
            for(int y = 0; y < sizeY; ++y)
                this.Columns.Add(new Column()
                {
                    Location = new Location()
                    {
                        X = x,
                        Y = y
                    }
                });
        }
        
        public void Initialize(List<IInput> inputs, double radius, double connectedPermanence)
        {
            this.ConnectedPermanence = connectedPermanence;
            
            foreach(Column column in Columns)
            {
                List<IInput> tempInputs = inputs.Where(i => column.Location.Distance(i.Location) <= radius).ToList();
                
                Random rnd = Processor.CreateRandom();
                List<Synapse> synapses = column.Synapses;
                
                //for(int i = 0; i < InitialNbSynapses && tempInputs.Count > 0; ++i)
                foreach(IInput selectedInput in tempInputs)
                {/*
                    int index = rnd.Next(0, tempInputs.Count);
                    IInput selectedInput = tempInputs[index];
                    tempInputs.RemoveAt(index);*/
                    double dist = column.Location.Distance(selectedInput.Location);
                    double perm = (rnd.NextDouble() - rnd.NextDouble()) * InitialRandomPermanenceMax + ConnectedPermanence - dist / InitialDistanceBiasDivider;
                    synapses.Add(new Synapse()
                    {
                        Input = selectedInput,
                        ConnectionThreshold = ConnectedPermanence,
                        Permanence = perm
                    });
                    //Console.WriteLine(perm);
                }
            }
        }
        
        public double ConnectedPermanence
        {
            get;
            set;
        }
        
        // = 0.1
        public double InitialRandomPermanenceMax
        {
            get;
            set;
        }
        public int InitialNbSynapses
        {
            get;
            set;
        }
        // 100
        public double InitialDistanceBiasDivider
        {
            get;
            set;
        }
        
        public List<Column> Columns
        {
            get;
            set;
        }
        
        public DendriteSegment Segment
        {
            get;
            set;
        }
        
        public double InhibitionRadius
        {
            get;
            set;
        }
        
        public int MinOverlap
        {
            get;
            set;
        }
        
        public int DesiredLocalActivity
        {
            get;
            set;
        }
        
        protected List<Column> ActiveColumns
        {
            get;
            set;
        }
        
        public double PermanenceInc
        {
            get;
            set;
        }
        
        public double PermanenceDec
        {
            get;
            set;
        }
        
        public double BoostStep
        {
            get;
            set;
        }
        
        public void Phase1Overlap()
        {
            foreach(Column column in Columns)
            {
                double overlap = column.Synapses
                    .Where(s => s.IsConnected)
                    .Where(s => s.Value)
                    .Count();
                
                //Console.WriteLine(column.Overlap + " " + overlap + " " + column.Synapses.Count + " " + column.Synapses.Where(s => s.IsConnected).Count());
                if(overlap < MinOverlap)
                    overlap = 0;
                else
                    overlap *= column.Boost;
                
                column.Overlap = overlap;
            }
            //foreach(Column column in Columns)
        }
        
        public List<Column> Neighbors(Column column, double radius)
        {
            return Columns
                //.Where(c => c != column)
                .Where(c => column.Location.Distance(c.Location) <= radius)
                .ToList();
        }
        
        public double GetKthBest(List<Column> columns, int kth)
        {
            return columns
                .OrderBy(c => c.Overlap)
                .Reverse()
                .Skip(kth - 1)
                .Select(c => c.Overlap)
                .First();
        }
        
        public void Phase2Inhibition()
        {
            ActiveColumns.Clear();
            foreach(Column column in Columns)
            {
                //Console.WriteLine(InhibitionRadius);
                double minLocalOverlap = GetKthBest(Neighbors(column, InhibitionRadius), DesiredLocalActivity);
                //Console.WriteLine(column.Overlap + " " + minLocalOverlap);
                if(column.Overlap > 0 && column.Overlap >= minLocalOverlap)
                {
                    ActiveColumns.Add(column);
                    column.Value = true;
                }
                else
                    column.Value = false;
            }
        }
        
        protected double MaxDutyCycle(List<Column> columns)
        {
            return columns
                .OrderBy(c => c.DutyCycle)
                .Reverse()
                .Select(c => c.DutyCycle)
                .First();
        }
        
        public void Phase3Learning()
        {
            foreach(Column activeColumn in ActiveColumns)
            {
                foreach(Synapse syn in activeColumn.Synapses)
                {
                    if(syn.IsConnected)
                        syn.Permanence += PermanenceInc;
                    else
                        syn.Permanence -= PermanenceDec;
                }
            }
            
            foreach(Column column in Columns)
            {
                double minDutyCycle = 0.01 * MaxDutyCycle(Neighbors(column, InhibitionRadius));
                
                if(column.DutyCycle > minDutyCycle)
                    column.Boost = 1;
                else
                    column.Boost += BoostStep;
                
                if(column.OverlapDutyCycle < minDutyCycle)
                    column.Synapses.ForEach(s => s.Permanence += 0.1 * ConnectedPermanence);
            }
            
            InhibitionRadius = Columns
                .SelectMany(c => c.Synapses
                    .Where(s => s.IsConnected)
                    .Select(s => c.Location.Distance(s.Location)))
                .DefaultIfEmpty(1.0)
                .Average();
            Console.WriteLine(" --*-* " + InhibitionRadius);
        }
        
        public bool[,] GetOutputMap()
        {
            int minX = Columns.Min(c => c.Location.X);
            int minY = Columns.Min(c => c.Location.Y);
            int maxX = Columns.Max(c => c.Location.X);
            int maxY = Columns.Max(c => c.Location.Y);
            
            bool[,] output = new bool[maxX - minX + 1, maxY - minY + 1];
            
            foreach(Column column in Columns)
            {
                
                    //Console.WriteLine(column.Overlap + " " + column.Value);
                output[column.Location.X - minX, column.Location.Y - minY] = column.Value;
            }
            
            return output;
        }
        
        public bool[,] GetResultingInputMap()
        {
            int minX = Columns.Min(c => c.Location.X);
            int minY = Columns.Min(c => c.Location.Y);
            int maxX = Columns.Max(c => c.Location.X);
            int maxY = Columns.Max(c => c.Location.Y);
            
            bool[,] output = new bool[maxX - minX + 1, maxY - minY + 1];
            
            foreach(List<Synapse> syns in Columns.Where(c => c.Value).Select(c => c.Synapses))
            {
                foreach(Synapse syn in syns.Where(s => s.IsConnected))
                    output[syn.Location.X - minX, syn.Location.Y - minY] = true;
            }
            
            return output;
        }
    }
}