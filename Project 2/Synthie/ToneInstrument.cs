﻿


using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Synthie
{
    public class ToneInstrument : Instrument
    {
        private double duration;
        private SineWave sinewave = new SineWave();
        private double amplitude;
        private double time;
        private AR ar = new AR();
        private float attack = 0.15f;
        private float decay = 0.15f;
        private float sustain = 0.40f;
        private float release = 0.20f;
        public double Frequency { get => sinewave.Frequency; set => sinewave.Frequency = value; }
        public double Amplitude { get => amplitude; set => amplitude = value; }

        public double Duration { get => duration; }
        public ToneInstrument()
        {
            duration = 0.1;
        }
        public override void SetNote(Note note,double secperbeat)
        {
            
            duration = note.Count;

            this.SecsPerBeat = secperbeat;
            Frequency = note.Freq;
            Amplitude = note.Amplitude;
        }
        public override bool Generate()
        {
            // Tell the component to generate an audio sample
            sinewave.Generate();
            frame = sinewave.Frame();

            float gain;

            if (time < attack)
                gain = (float)time / attack;
            else if (time < attack + decay)
                gain = 1 - (((float)time - attack) / decay) * (1 - sustain);
            else if (time < duration - release)
                gain = sustain;
            else
                gain = sustain * ((float)duration - (float)time) / release;

          
            frame[0] = gain * frame[0];
            frame[1] = gain * frame[1];


            // Read the component's sample and make it our resulting frame.

            // Update time
            time += samplePeriod;

            // We return true until the time reaches the duration.
            return time < duration * this.SecsPerBeat;
        }

        public override void Start()
        {
            sinewave.SampleRate = SampleRate;
            sinewave.Start();
            time = 0;

            // Tell the AR object where it gets its samples from 
            // the sine wave object.
            ar.Source = this;
            ar.SampleRate = SampleRate;
            ar.Duration = duration;
            ar.SamplePeriod = samplePeriod;
            ar.Amplitude = amplitude;
            ar.Start();
        }
    }
}
