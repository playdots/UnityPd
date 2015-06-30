#ifndef STK_ADSR_H
#define STK_ADSR_H

#include "maximilian.h"

/***************************************************/
/*! \class ADSR
 \brief STK ADSR envelope class.
 
 This class implements a traditional ADSR (Attack, Decay, Sustain,
 Release) envelope.  It responds to simple keyOn and keyOff
 messages, keeping track of its state.  The \e state = ADSR::IDLE
 before being triggered and after the envelope value reaches 0.0 in
 the ADSR::RELEASE state.  All rate, target and level settings must
 be non-negative.  All time settings are in seconds and must be
 positive.
 
 by Perry R. Cook and Gary P. Scavone, 1995--2014.
 */
/***************************************************/

class ADSR
{
public:
    
    //! ADSR envelope states.
    enum {
        ATTACK,   /*!< Attack */
        DECAY,    /*!< Decay */
        SUSTAIN,  /*!< Sustain */
        RELEASE,  /*!< Release */
        IDLE      /*!< Before attack / after release */
    };
    
    //! Default constructor.
    ADSR( void );
    
    //! Class destructor.
    ~ADSR( void );
    
    //! Set target = 1, state = \e ADSR::ATTACK.
    void trigger( void );
        
    //! Set the attack rate (gain / sample).
    void setAttackRate( float rate );
    
    //! Set the target value for the attack (default = 1.0).
    void setAttackTarget( float target );
    
    //! Set the decay rate (gain / sample).
    void setDecayRate( float rate );
    
    //! Set the sustain level.
    void setSustainLevel( float level );
    
    //! Set the release rate (gain / sample).
    void setReleaseRate( float rate );
    
    //! Set the attack rate based on a time duration (seconds).
    void setAttackTime( float time );
    
    //! Set the decay rate based on a time duration (seconds).
    void setDecayTime( float time );
    
    //! Set the release rate based on a time duration (seconds).
    void setReleaseTime( float time );
    
    //! Set the hold time of the note (seconds).
    void setSustainTime( float time );
    
    //! Set sustain level and attack, decay, and release time durations (seconds).
    void setAllTimes( float aTime, float dTime, float sLevel, float rTime );
    
    //! Set a sustain target value and attack or decay from current value to target.
    void setTarget( float target );
    
    //! Return the current envelope \e state (ATTACK, DECAY, SUSTAIN, RELEASE, IDLE).
    int getState( void ) const { return state_; };
    
    //! Set to state = ADSR::SUSTAIN with current and target values of \e value.
    void setValue( float value );
    
    //! Compute and return one output sample.
    float tick( void );
    
    
protected:
    
    void sampleRateChanged( float newRate, float oldRate );
    
    int state_;
    float value_;
    float target_;
    float attackRate_;
    float decayRate_;
    float releaseRate_;
    float releaseTime_;
    float sustainLevel_;
    float sustainCounter_;
    float sustainTime_;

};

inline float ADSR :: tick( void )
{
    switch ( state_ ) {
            
        case ATTACK:
            value_ += attackRate_;
            if ( value_ >= target_ ) {
                value_ = target_;
                target_ = sustainLevel_;
                state_ = DECAY;
            }
            break;
            
        case DECAY:
            if ( value_ > sustainLevel_ ) {
                value_ -= decayRate_;
                if ( value_ <= sustainLevel_ ) {
                    value_ = sustainLevel_;
                    state_ = SUSTAIN;
                }
            }
            else {
                value_ += decayRate_; // attack target < sustain level
                if ( value_ >= sustainLevel_ ) {
                    value_ = sustainLevel_;
                    state_ = SUSTAIN;
                }
            }
            break;
            
        case SUSTAIN:
            sustainCounter_++;
            if (sustainCounter_ >= sustainTime_){
                state_ = RELEASE;
                sustainCounter_ = 0;
                if ( releaseTime_ > 0.0 )
                    releaseRate_ = value_ / ( maxiSettings::sampleRate );
            }
            break;
            
        case RELEASE:
            value_ -= releaseRate_;
            if ( value_ <= 0.0 ) {
                value_ = 0.0;
                state_ = IDLE;
            }
            
    }
    
    return value_;
}

#endif
