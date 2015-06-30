//
//  Synth.h
//  AudioPluginDemo
//
//  Created by yotam mann on 6/29/15.
//
//

#ifndef __AudioPluginDemo__Synth__
#define __AudioPluginDemo__Synth__

#include "maximilian.h"
#include "ADSR.h"

#define POLYPHONY 8

class Synth {
    
    enum OSC_TYPE {
        SINE,
        TRIANGLE,
        SAWTOOTH,
        SQUARE
    };
    
public:
    
    Synth();
    ~Synth();
    //called every frame
    float tick();
    
    void triggerNote(float freq);
    
    void setEnvelope(float a, float d, float s, float h, float r);
    
    void setSampleRate(int sampleRate);
    
    void setOscType(int type);

private:

    
    //the oscillators and envelopes
    ADSR envelopes[POLYPHONY];
    float oscPitches[POLYPHONY];
    maxiOsc oscillators[POLYPHONY];
    
    int oscType;
    
    //which voice we're currently playing
    int voice;

};

#endif /* defined(__AudioPluginDemo__Synth__) */
