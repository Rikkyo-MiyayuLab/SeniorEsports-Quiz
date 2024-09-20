using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public UnityEvent onTimerEnd;
    public UnityEvent onTimerStart;

    [Range(0, 364)]
    public int days;
    [Range(0, 23)]
    public int hours;
    [Range(0, 59)]
    public int minutes;
    [Range(0, 59)]
    public int seconds;


    public enum CountMethod
    {
        CountDown,
        CountUp
    };
    
    public enum SeperatorType
    {
        Colon,
        Bullet,
        Slash,
        TimeCharacter
    };
    public enum OutputType
    {
        None,
        StandardText,
        TMPro,
        HorizontalSlider,
        Dial
    };

    [Tooltip("If checked, runs the timer on play")]
    public bool startAtRuntime = true; 

    [Tooltip("If checked, use timer as a digital clock with the current time, remember to choose to count up")]
    public bool useAsClock;

    [Tooltip("Select what to display")]
    public bool daysDisplay = false;
    public bool hoursDisplay = false;
    public bool minutesDisplay = true;
    public bool secondsDisplay = true;
    public bool millisecondsDisplay = true;

    [Tooltip("Will remove days, hours, minutes display if there would be 0. Example - Display 59 instead of 00:59 during countdown")]
    public bool adaptiveDisplay = false;

    public bool removeLeadingZeros = false;

    [Tooltip("Changes timed text into a percentage")]
    public bool percentageDisplay = false;

    [Range(0.1f, 100f)]
    public float timerSpeed = 1f;

    [Space]
    
    [Tooltip("Select to count up or down")]
    public CountMethod countMethod;
    
    [Tooltip("Select the seperator type")]
    public SeperatorType seperatorType;

    [Tooltip("Select the output type")]
    public OutputType outputType;
    public Text standardText;
    public TextMeshProUGUI textMeshProText;
    public Slider standardSlider;
    public Image dialSlider;

    public bool repeat;

    bool timerRunning = false;
    bool timerPaused = false;
    public double timeRemaining;
    

    private void Awake()
    {
        if(!standardText)
        if(GetComponent<Text>())
        {
            standardText = GetComponent<Text>();
        }
        if(!textMeshProText)
        if(GetComponent<TextMeshProUGUI>())
        {
            textMeshProText = GetComponent<TextMeshProUGUI>();
        }
        if(!standardSlider)
        if(GetComponent<Slider>())
        {
            standardSlider = GetComponent<Slider>();
        }
        if(!dialSlider)
        if(GetComponent<Image>())
        {
            dialSlider = GetComponent<Image>();
        }
        if(standardSlider)
        {
            standardSlider.maxValue = ReturnTotalSeconds();
            if(countMethod == CountMethod.CountDown)
            {
                standardSlider.value = standardSlider.maxValue;
            }
            else
            {
                standardSlider.value = standardSlider.minValue;
            }
        }
        if(dialSlider)
        {
            if (countMethod == CountMethod.CountDown)
            {
                dialSlider.fillAmount = 1f;
            }
            else
            {
                dialSlider.fillAmount = 0f;
            }
        }
    }
    void Start()
    {
        if (repeat) onTimerEnd.AddListener(() => ReStartTimer());
        if(startAtRuntime)
        {
            StartTimer();
        }
        else
        {
            if(countMethod == CountMethod.CountDown)
            {
                if(standardText)
                {
                    standardText.text = DisplayFormattedTime(ReturnTotalSeconds());
                }
                if(textMeshProText)
                {
                    textMeshProText.text = DisplayFormattedTime(ReturnTotalSeconds());
                }
            }
            else
            {
                if (standardText)
                {
                    standardText.text = DisplayFormattedTime(0);
                }
                if (textMeshProText)
                {
                    textMeshProText.text = DisplayFormattedTime(0);
                }
            }
        }
    }

    private void ReStartTimer()
    {
        Debug.Log("Restart");
        StopTimer();
        StartTimer();
    }

    void Update()
    {
        if(Input.GetKeyDown("p"))
        {
            PauseTimer();
        }
        if(timerRunning)
        {
            if(countMethod == CountMethod.CountDown)
            {
                CountDown();
                if(standardSlider && !useAsClock)
                {
                    StandardSliderDown();
                }
                if(dialSlider && !useAsClock)
                {
                    DialSliderDown();
                }
            }
            else
            {
                CountUp();
                if (standardSlider && !useAsClock)
                {
                    StandardSliderUp();
                }
                if(dialSlider && !useAsClock)
                {
                    DialSliderUp();
                }
            }
        }
    }

    private void CountDown()
    {
        /*If you choose to edit this back to 0 for 100% accuracy,
        1 frame at the end of the timer will display maximum numbers as it takes time to switch to the else statement
        which sets the time remaining to 0. This is accurate up to 20 milliseconds or 0.02 of a second.*/  
        if (timeRemaining > 0.02)
        {
            timeRemaining -= Time.deltaTime * timerSpeed;
            DisplayInTextObject();
        }
        else
        {
            //Timer has ended from counting downwards
            timeRemaining = 0;
            timerRunning = false;
            if(standardSlider) standardSlider.value = 0;
            onTimerEnd.Invoke();
            DisplayInTextObject();
        }
    }

    private void CountUp()
    {
        if(useAsClock)
        {
            timeRemaining += Time.deltaTime * timerSpeed;
            DisplayInTextObject();
            return;
        }
        if (timeRemaining < ReturnTotalSeconds())
        {
            timeRemaining += Time.deltaTime * timerSpeed;
            DisplayInTextObject();
        }
        else
        {
            //Timer has ended from counting upwards
            timeRemaining = ReturnTotalSeconds();
            DisplayInTextObject();
            timerRunning = false;
            onTimerEnd.Invoke();
        }
    }
    private void StandardSliderDown()
    {
        if(standardSlider.value > standardSlider.minValue)
        {
            standardSlider.value -= Time.deltaTime * timerSpeed;
        }
    }
    private void StandardSliderUp()
    {
        if (standardSlider.value < standardSlider.maxValue)
        {
            standardSlider.value += Time.deltaTime * timerSpeed;
        }
    }
    private void DialSliderDown()
    {
        float timeRangeClamped = Mathf.InverseLerp(ReturnTotalSeconds(), 0, (float)timeRemaining);
        dialSlider.fillAmount = Mathf.Lerp(1, 0, timeRangeClamped);
    }
    private void DialSliderUp()
    {
        float timeRangeClamped = Mathf.InverseLerp(0, ReturnTotalSeconds(), (float)timeRemaining);
        dialSlider.fillAmount = Mathf.Lerp(0, 1, timeRangeClamped);
    }
    private void DisplayInTextObject()
    {
        if (standardText)
        {
            standardText.text = DisplayFormattedTime(timeRemaining);
        }
        if (textMeshProText)
        {
            textMeshProText.text = DisplayFormattedTime(timeRemaining);
        }
    }

    public double GetRemainingSeconds()
    {
        return timeRemaining;
    }
    public void StartTimer()
    {
        if (!timerRunning && !timerPaused)
        {
            ResetTimer();
            if (useAsClock)
            {
                ConvertToTotalSeconds(0, System.DateTime.Now.Hour, System.DateTime.Now.Minute, System.DateTime.Now.Second);
                onTimerStart.Invoke();
                timerRunning = true;
                return;
            }
            if (countMethod == CountMethod.CountDown)
            {
                ConvertToTotalSeconds(days, hours, minutes, seconds);
                onTimerStart.Invoke();
            }
            else
            {
                StartTimerCustom(0);
            }
            timerRunning = true;
        }
    }
    public void StartTimerCustom(double timeToSet)
    {
        double validatedTimeToSet = ValidateTimeToSet(timeToSet);
        if (!timerRunning && !timerPaused)
        {
            //Debug.Log("Repeat");
            onTimerStart.Invoke();
            if (countMethod == CountMethod.CountDown)
            {
                timeRemaining = validatedTimeToSet;
            }
            else
            {
            Debug.Log(validatedTimeToSet);
                timeRemaining = validatedTimeToSet;
                if (standardSlider)
                {
                    standardSlider.maxValue = ReturnTotalSeconds();
                    standardSlider.value = (float)timeRemaining;
                }
            }
            timerRunning = true;
        }
    }

    private double ValidateTimeToSet(double timeToSet)
    {
        if (timeToSet <= 0) return 0;
        if (timeToSet > ReturnTotalSeconds()) return ReturnTotalSeconds();
        return timeToSet;
    }

    public void StopTimer()
    {
        timerRunning = false;
        ResetTimer();
    }
    public void PauseTimer()
    {
        timerRunning = false;
        timerPaused = true;
    }
    public void ResumeTimer()
    {
        if(timerPaused)
        {
            timerRunning = true;
            timerPaused = false;
        }
    }
    private void ResetTimer()
    {
        timerPaused = false;
        
        if (countMethod == CountMethod.CountDown)
        {
            timeRemaining = ReturnTotalSeconds();
            DisplayInTextObject();
            if(standardSlider)
            {
                standardSlider.maxValue = ReturnTotalSeconds();
                standardSlider.value = standardSlider.maxValue;
            }
            if(dialSlider)
            {
                dialSlider.fillAmount = 1f;
            }
        }
        else
        {
            timeRemaining = 0;
            DisplayInTextObject();
            if (standardSlider)
            {
                standardSlider.maxValue = ReturnTotalSeconds();
                standardSlider.value = standardSlider.minValue;
            }
            if (dialSlider)
            {
                dialSlider.fillAmount = 0f;
            }
        }
    }
    public void AddTime(float secondsToAdd)
    {
        if (timerRunning)
        {
            if (countMethod == CountMethod.CountDown && (secondsToAdd + timeRemaining) < ReturnTotalSeconds())
            {
                timeRemaining += secondsToAdd;
                if (standardSlider)
                {
                    standardSlider.value += secondsToAdd;
                }
            }
            if (countMethod == CountMethod.CountUp)
            {
                timeRemaining += secondsToAdd;
                if (standardSlider)
                {
                    standardSlider.value += secondsToAdd;
                }
            }
        }    
    }

    public void RemoveTime(float secondsToRemove)
    {
        if(timerRunning)
        {
            if(countMethod == CountMethod.CountUp && timeRemaining >= secondsToRemove)
            {
                timeRemaining -= secondsToRemove;
                if (standardSlider)
                {
                    standardSlider.value -= secondsToRemove;
                }
            }
            if(countMethod == CountMethod.CountDown)
            {
                timeRemaining -= secondsToRemove;
                if (standardSlider)
                {
                    standardSlider.value -= secondsToRemove;
                }
            }
        }
    }  
    
    public float ReturnTotalSeconds()
    {
        float totalTimeSet;
        totalTimeSet = days * 24 * 60 * 60;
        totalTimeSet += hours * 60 * 60;
        totalTimeSet += minutes * 60;
        totalTimeSet += seconds;
        return totalTimeSet;
    }
   
    public double ConvertToTotalSeconds(float days, float hours, float minutes, float seconds)
    {
        timeRemaining = days * 24 * 60 * 60;
        timeRemaining += hours * 60 * 60;
        timeRemaining += minutes * 60;
        timeRemaining += seconds;

        DisplayFormattedTime(timeRemaining);
        return timeRemaining;
    }
    public string DisplayFormattedTime(double remainingSeconds)
    {
        string convertedNumber;
        float days, hours, minutes, seconds;
        double milliseconds;
        RemainingSecondsToDHHMMSSMMM(remainingSeconds, out days, out hours, out minutes, out seconds, out milliseconds);

        if (!percentageDisplay)
        {
            string DaysFormat()
            {
                if (daysDisplay)
                {
                    string daysFormatted;
                    if(removeLeadingZeros)
                    {
                        daysFormatted = string.Format("{0:0}", days);
                    }
                    else
                    {
                        daysFormatted = string.Format("{0:000}", days);
                    }

                    if (seperatorType == SeperatorType.TimeCharacter) daysFormatted += "d ";
                    else if (hoursDisplay || minutesDisplay || secondsDisplay || millisecondsDisplay)
                        daysFormatted += ":";

                    

                    if(adaptiveDisplay && days == 0)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return daysFormatted;
                    }
                }
                return null;
            }
            string HoursFormat()
            {
                if (hoursDisplay)
                {
                    string hoursFormatted;
                    if(removeLeadingZeros)
                    {
                        hoursFormatted = string.Format("{0:0}", hours);
                    }
                    else
                    {
                        hoursFormatted = string.Format("{0:00}", hours);
                    }

                    if(seperatorType == SeperatorType.TimeCharacter) hoursFormatted += "h ";
                    else if (minutesDisplay || secondsDisplay || millisecondsDisplay)
                        hoursFormatted += ":";


                    if (adaptiveDisplay && hours == 0)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return hoursFormatted;
                    }
                }
                return null;
            }
            string MinutesFormat()
            {
                if (minutesDisplay)
                {
                    string minutesFormatted;
                    if(removeLeadingZeros)
                    {
                        minutesFormatted = string.Format("{0:0}", minutes);
                    }
                    else
                    {
                        minutesFormatted = string.Format("{0:00}", minutes);
                    }

                    if (seperatorType == SeperatorType.TimeCharacter) minutesFormatted += "m ";
                    else if (secondsDisplay || millisecondsDisplay)
                        minutesFormatted += ":";
                               

                    if (adaptiveDisplay && minutes == 0)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return minutesFormatted;
                    }
                }
                return null;
            }
            string SecondsFormat()
            {
                if (secondsDisplay)
                {
                    string secondsFormatted; 
                    if(removeLeadingZeros)
                    {
                        secondsFormatted = string.Format("{0:0}", seconds);
                    }
                    else
                    {
                        secondsFormatted = string.Format("{0:00}", seconds);
                    }
                    if (seperatorType == SeperatorType.TimeCharacter) secondsFormatted += "s";
                    else if (millisecondsDisplay)
                        secondsFormatted += ":";


                    return secondsFormatted;
                }
                return null;
            }
            string MillisecondsFormat()
            {
                if (millisecondsDisplay)
                {
                    string millisecondsFormatted;
                    millisecondsFormatted = string.Format("{0:000}", milliseconds);
                    return millisecondsFormatted;
                }
                return null;
            }

            convertedNumber = DaysFormat() + HoursFormat() + MinutesFormat() + SecondsFormat() + MillisecondsFormat();
            convertedNumber = Seperator(convertedNumber);
        }

        else
        {
            if(countMethod == CountMethod.CountDown)
            {
                float timeRangeClamped = Mathf.InverseLerp(0, ReturnTotalSeconds(), (float)timeRemaining);
                convertedNumber = Mathf.Lerp(0, 100, timeRangeClamped).ToString("F0") + "%";
            }
            else
            {
                float timeRangeClamped = Mathf.InverseLerp(ReturnTotalSeconds(), 0, (float)timeRemaining);
                convertedNumber = Mathf.Lerp(100, 0, timeRangeClamped).ToString("F0") + "%";
            }
            
        }
        return convertedNumber;
    }

    private static void RemainingSecondsToDHHMMSSMMM(double totalSeconds, out float days, out float hours, out float minutes, out float seconds, out double milliseconds)
    {
        days = Mathf.FloorToInt((float)totalSeconds / 24 / 60 / 60);
        hours = Mathf.FloorToInt(((float)totalSeconds / 60 / 60) - ((float)days * 24));
        minutes = Mathf.FloorToInt(((float)totalSeconds / 60) - ((float)days * 24 * 60) - ((float)hours * 60));
        seconds = Mathf.FloorToInt((float)totalSeconds - ((float)days * 24 * 60 * 60) - ((float)hours * 60 * 60) - ((float)minutes * 60));
        milliseconds = (totalSeconds % 1) * 1000;
    }

    private string Seperator(string convertedNumber)
    {
        if (seperatorType == SeperatorType.Bullet)
        {
            convertedNumber = convertedNumber.Replace(":", ".");
        }
        if (seperatorType == SeperatorType.Slash)
        {
            convertedNumber = convertedNumber.Replace(":", "/");
        }

        return convertedNumber;
    }
    private void OnValidate()
    {
        timeRemaining = ConvertToTotalSeconds(days, hours, minutes, seconds);
    }
}
