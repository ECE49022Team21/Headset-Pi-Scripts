#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>
#include <time.h>

#include "./I2Clib/pi_i2c.h"
#include "./I2Clib/GPIOlib/get_pi_version.h" // Determines PI versions

//ADC Constants
const int ADC_Address = 0x54;
const int Reg_Conversion_Result = 0b000;
const int Reg_Alert_Status = 0b001;
const int Reg_Configuration = 0b010;
const int Reg_Low_Limit = 0b011;
const int Reg_High_Limit = 0b100;
const int Reg_Hysteresis = 0b101;
const int Reg_Lowest_Conversion = 0b110;
const int Reg_Highest_Conversion = 0b111;

//DAC Constants
const int DAC_Address = 0x60;
const int Reg_Dac_Output = 0x000;


double avgReadTime;

int Scan_I2C_Bus()
{
    int address_book[127];

    int ret;

    if ((ret = scan_bus_i2c(address_book)) < 0)
    {
        printf("Error! i2c bus scan returned error code: %d\n\n", ret);
        return -1;
    }

    // Check and see if ADC was detected on the bus
    if (address_book[ADC_Address] != 1)
    {
        printf("ADC was not detected at 0x%X\n", ADC_Address);
        return -1;
    }

    // Check and see if DAC was detected on the bus
    if (address_book[DAC_Address] != 1)
    {
        printf("DAC was not detected at 0x%X\n", DAC_Address);
        return -1;
    }

    printf("ADC was detected at 0x%X\n", ADC_Address);
    printf("DAC was detected at 0x%X\n", DAC_Address);

    return 0;
}

void Write(int deviceAddress, int register_address, int *data, int n_bytes)
{
    int ret = write_i2c(deviceAddress, register_address, data, n_bytes);
    if (ret < 0)
        printf("Error! i2c write returned error code: %d\n\n", ret);
}

void Read(int deviceAddress, int register_address, int *data, int n_bytes, int setRegisterBool)
{
    for (int i = 0; i < n_bytes; i++)
    {
        data[i] = 0;
    }
    int ret = read_i2c(deviceAddress, register_address, data, n_bytes, setRegisterBool);
    if (ret < 0)
        printf("Error! i2c read returned error code: %d\n\n", ret);
}

void ConfigureADC()
{
    int data[1] = {0b01000000};
    int n_bytes = 1;
    Write(ADC_Address, Reg_Configuration, data, n_bytes);

    // set ADC register address with a read
    Read(ADC_Address, Reg_Conversion_Result, data, n_bytes, 1);
}

float GetAudioValue(int rawValue)
{
    int voltageStep = (rawValue >> 2) & 0b1111111111;
    float audio = (((float)2 / 1024) * voltageStep) - 1;
    return audio;
}

int GetRawAudioValue()
{
    int data[2];
    int n_bytes = 2;
    Read(ADC_Address, Reg_Conversion_Result, data, n_bytes, 0);
    return ((data[0] << 8) | data[1]);
}

float* RecordAudio(int seconds)
{
    clock_t start, end;
    double execution_time;
    float *audioValues = malloc(8000 * seconds * sizeof(float));
    int rawValues[8000 * seconds];
    start = clock();
    for (int i = 0; i < 8000 * seconds; i++)
    {
        rawValues[i] = GetRawAudioValue();
    }
    end = clock();
    execution_time = ((double)(end - start)) / CLOCKS_PER_SEC;
    float sampleRate = (8000 * seconds) / execution_time;
    printf("Sample Rate: %f\n", sampleRate);

    for (int i = 0; i < 8000 * seconds; i++)
    {
        audioValues[i] = GetAudioValue(rawValues[i]);
        printf("Audio Value: %f\n", audioValues[i]);
    }

    return audioValues;
}

void PlayAudio(float* audioValues)
{
    printf("%d\n", sizeof(audioValues) / sizeof(float));
    int rawAudioValues[sizeof(audioValues) / sizeof(float)];

    for (int i = 0; i < (sizeof(audioValues) / sizeof(float)); i++)
    {
        rawAudioValues[i] = ((audioValues[i] + 1) * 4096) / 2;
    }

    for (int i = 0; i < (sizeof(audioValues) / sizeof(float)); i++)
    {
        int data[2];
        data[1] = 0b11111111 & rawAudioValues[i];
        data[0] = 0b1111 & (rawAudioValues[i] >> 8);
        Write(DAC_Address, Reg_Dac_Output, data, 2);
    }
}

int main()
{
    printf("Initializing Audio Collection\n");
    // Use the default I2C pins:
    // Ensure that Raspian I2C interface is disabled via rasp-config otherwise
    // risk unpredictable behavior!
    int sda_pin = 2;
    int scl_pin = 3;

    int speed_grade = I2C_FULL_SPEED;
    printf("I2C Clock Speed: %d Hz\n", speed_grade);

    // Display pi Version
    printf("Raspberry Pi Version: %d\n", get_pi_version__());

    // Configure at standard mode:
    int errCheck = config_i2c(sda_pin, scl_pin, speed_grade);
    if (errCheck < 0)
    {
        printf("I2C configuration error: %d\n", errCheck);
        return EXIT_FAILURE;
    }
    else
        printf("I2C configured\n");

    // Scan I2C bus and identify if device is present:
    if (Scan_I2C_Bus() < 0)
        return EXIT_FAILURE;

    ConfigureADC();
    printf("ADC Configured\n");
    
    printf("Recording Audio\n");
    float *audioValues = RecordAudio(5);

    // Open file:
    FILE *fd = fopen("./audioOut.binary", "w");

    // Write file:
    fwrite(&audioValues, sizeof(audioValues), 1, fd);

    // Close file:
    fclose(fd);

    printf("Playing Audio\n");
    PlayAudio(audioValues);

    free(audioValues);
}