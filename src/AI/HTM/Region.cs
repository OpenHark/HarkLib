using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HarkLib.AI.old
{
    // https://github.com/UKirsche/UIHtmCal/tree/simple/HTMLibrary/HTMLibrary
    // http://numenta.com/assets/pdf/whitepapers/hierarchical-temporal-memory-cortical-learning-algorithm-0.2.1-en.pdf
    public class Region
    {
        public Region()
        {
            
        }
        
        public void Initialize(List<IInput> inputs, double connectedPermanence)
        {
            this.ConnectedPermanence = connectedPermanence;
            
            foreach(Column column in Columns)
            {
                List<IInput> tempInputs = inputs.ToList();
                
                Random rnd = new Random();
                List<Synapse> synapses = column.Synapses;
                
                for(int i = 0; i < InitialNbSynapses; ++i)
                {
                    int index = rnd.Next(0, tempInputs.Count);
                    IInput selectedInput = tempInputs[index];
                    tempInputs.RemoveAt(index);
                    double dist = column.Location.Distance(selectedInput.Location);
                    synapses.Add(new Synapse()
                    {
                        Input = selectedInput,
                        ConnectionThreshold = ConnectedPermanence,
                        Permanence = dist / InitialDistanceBiasDivider + rnd.NextDouble() * InitialRandomPermanenceMax + ConnectedPermanence
                    });
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
        
        public double Boost(Column c)
        {
            return 1.0;
        }
        
        public void Phase1Overlap()
        {
            foreach(Column column in Columns)
            {
                double overlap = 0;
                
                foreach(Synapse syn in column.Synapses.Where(s => s.IsConnected))
                    overlap += syn.Value ? 1 : 0;
                
                if(overlap < MinOverlap)
                    overlap = 0;
                else
                    overlap *= column.Boost;
                
                column.Overlap = overlap;
            }
        }
        
        public int DesiredLocalActivity
        {
            get;
            set;
        }
        
        public List<Column> Neighbors(Column column, double radius)
        {
            return Columns
                .Where(c => c != column)
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
        
        protected List<Column> ActiveColumns
        {
            get;
            set;
        }
        
        public void Phase2Inhibition()
        {
            ActiveColumns.Clear();
            foreach(Column column in Columns)
            {
                double minLocalOverlap = GetKthBest(Neighbors(column, InhibitionRadius), DesiredLocalActivity);
                
                if(column.Overlap > 0 && column.Overlap >= minLocalOverlap)
                {
                    ActiveColumns.Add(column);
                    column.Value = true;
                }
                else
                    column.Value = false;
            }
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
            
            //InhibitionRadius = 
        }
        
        public double BoostStep
        {
            get;
            set;
        }
    }
}