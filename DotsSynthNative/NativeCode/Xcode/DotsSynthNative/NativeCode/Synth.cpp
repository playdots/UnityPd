//
//  Synth.cpp
//  AudioPluginDemo
//
//  Created by yotam mann on 6/29/15.
//
//

#include "Synth.h"

Synth::Synth(){
    oscType = SINE;
}

Synth::~Synth(){

}


void Synth::setSampleRate(int sampleRate){
    
    maxiSettings::sampleRate = sampleRate;
}


void Synth::setEnvelope(float attack, float decay, float sustain, float hold, float release){
    float db = pow(2, sustain / 6);
    for (int i = 0; i < POLYPHONY; i++){
        envelopes[i].setAllTimes(attack / 1000.0f, decay / 1000.0f, db, release / 1000.0f);
        envelopes[i].setSustainTime(hold / 1000.0f);
    }
}

void Synth::setOscType(int type){
    oscType = type;
}

float Synth::tick(){
    
    float sample = 0;
    
    for (int i = 0; i < POLYPHONY; i++){
        float oscillator = 0;
        //choose the oscillator from the enum
        switch (oscType) {
            case SINE:
                oscillator = oscillators[i].sinewave(oscPitches[i]);
                break;
            case TRIANGLE:
                oscillator = oscillators[i].triangle(oscPitches[i]);
                break;
            case SQUARE:
                oscillator = oscillators[i].square(oscPitches[i]);
                break;
            case SAWTOOTH:
                oscillator = oscillators[i].saw(oscPitches[i]);
                break;
                
            default:
                break;
        }
        sample += envelopes[i].tick() * oscillator;
    }
    return sample;
}

void Synth::triggerNote(float freq){
    //trigger a new note
    envelopes[voice].trigger();
    oscPitches[voice] =  freq;
    //increment the voice count
    voice++;
    if (voice == POLYPHONY){
        voice = 0;
    }
}