/***************************************************/
/*! \class ADSR
 \brief STK ADSR envelope class.
 
 This class implements a traditional ADSR (Attack, Decay, Sustain,
 Release) envelope.  It responds to simple keyOn and keyOff
 messages, keeping track of its state.  The \e state = ADSR::IDLE
 before being triggered and after the envelope value reaches 0.0 in
 the ADSR::RELEASE state.  All rate, target and level settings must
 be non-negative.  All time settings must be positive.
 
 by Perry R. Cook and Gary P. Scavone, 1995--2014.
 */
/***************************************************/

#include "ADSR.h"


ADSR :: ADSR( void )
{
    target_ = 0.0;
    value_ = 0.0;
    attackRate_ = 0.001;
    decayRate_ = 0.001;
    releaseRate_ = 0.005;
    releaseTime_ = -1.0;
    sustainLevel_ = 0.5;
    sustainTime_ = 10000;
    state_ = IDLE;
}

ADSR :: ~ADSR( void )
{
}



void ADSR :: trigger()
{
    target_ = 1.0;
    state_ = ATTACK;
}


void ADSR :: setAttackRate( float rate )
{
    attackRate_ = rate;
}

void ADSR :: setAttackTarget( float target )
{
    target_ = target;
}

void ADSR :: setDecayRate( float rate )
{
    decayRate_ = rate;
}

void ADSR :: setSustainLevel( float level )
{
    sustainLevel_ = level;
}

void ADSR :: setSustainTime( float time )
{
    sustainTime_ = time * maxiSettings::sampleRate;
}

void ADSR :: setReleaseRate( float rate )
{
    releaseRate_ = rate;
    
    // Set to negative value so we don't update the release rate on keyOff()
    releaseTime_ = -1.0;
}

void ADSR :: setAttackTime( float time )
{
    attackRate_ = 1.0 / ( time * maxiSettings::sampleRate );
}

void ADSR :: setDecayTime( float time )
{
    decayRate_ = (1.0 - sustainLevel_) / ( time * maxiSettings::sampleRate );
}

void ADSR :: setReleaseTime( float time )
{
    
    releaseRate_ = sustainLevel_ / ( time * maxiSettings::sampleRate );
    releaseTime_ = time;
}

void ADSR :: setAllTimes( float aTime, float dTime, float sLevel, float rTime )
{
    this->setAttackTime( aTime );
    this->setSustainLevel( sLevel );
    this->setDecayTime( dTime );
    this->setReleaseTime( rTime );
}

void ADSR :: setTarget( float target )
{
    
    target_ = target;
    
    this->setSustainLevel( target_ );
    if ( value_ < target_ ) state_ = ATTACK;
    if ( value_ > target_ ) state_ = DECAY;
}

void ADSR :: setValue( float value )
{
    state_ = SUSTAIN;
    target_ = value;
    value_ = value;
    this->setSustainLevel( value );
}