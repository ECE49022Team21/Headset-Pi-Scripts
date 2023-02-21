#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>

#include "./I2Clib/pi_i2c.h"
#include "./I2Clib/GPIOlib/get_pi_version.h"     // Determines PI versions

const int ADC_Address = 0x54;
const int Reg_Conversion_Result = 0b000;
const int Reg_Alert_Status = 0b001;
const int Reg_Configuration = 0b010;
const int Reg_Low_Limit = 0b011;
const int Reg_High_Limit = 0b100;
const int Reg_Hysteresis = 0b101;
const int Reg_Lowest_Conversion = 0b110;
const int Reg_Highest_Conversion = 0b111;


int Scan_I2C_Bus()
{
    int address_book[127];

    int ret;

    if ((ret = scan_bus_i2c(address_book)) < 0) {
        printf("Error! i2c bus scan returned error code: %d\n\n", ret);
        return -1;
    }

    // Check and see if ADC was detected on the bus
    if (address_book[ADC_Address] != 1) {
        printf("ADC was not detected at 0x%X\n", ADC_Address);
        return -1;
    }

    printf("ADC was detected at 0x%X\n", ADC_Address);

    return 0;
}

void Write(int register_address, int *data, int n_bytes)
{
    int ret = write_i2c(ADC_Address, register_address, data, n_bytes);
    if (ret < 0) printf("Error! i2c write returned error code: %d\n\n", ret);
}

void Read(int register_address, int *data, int n_bytes)
{
    for (int i = 0; i < n_bytes; i++) { data[i] = 0; }
    int ret = read_i2c(ADC_Address, register_address, data, n_bytes);
    if (ret < 0) printf("Error! i2c read returned error code: %d\n\n", ret);
}

void ConfigureADC()
{
    int data[1] = {0b01000000};
    int n_bytes = 1;
    Write(Reg_Configuration, data, n_bytes);
}

float GetVoltageValue()
{
    int data[2];
    int n_bytes = 2;
    Read(Reg_Conversion_Result, data, n_bytes);
    int voltageStep = ((data[0] << 8) | data[1]);
    voltageStep = (voltageStep >> 2) & 0b1111111111;
    float voltage = ((float)3.3 / 1024) * voltageStep;
    return voltage;
}

int main()
{
    printf("Initializing Audio Collection\n");
    // Use the default I2C pins:
    // Ensure that Raspian I2C interface is disabled via rasp-config otherwise
    // risk unpredictable behavior!
    int sda_pin = 2;
    int scl_pin = 3;

    int speed_grade = I2C_STANDARD_MODE;
    printf("I2C Clock Speed: %d Hz\n", speed_grade);

    //Display pi Version
    printf("Raspberry Pi Version: %d\n", get_pi_version__());

    // Configure at standard mode:
    int errCheck = config_i2c(sda_pin, scl_pin, speed_grade);
    if (errCheck < 0)
    {
        printf("I2C configuration error: %d\n", errCheck);
        return EXIT_FAILURE;
    }
    else printf("I2C configured\n");

    // Scan I2C bus and identify if device is present:
    if (Scan_I2C_Bus() < 0) return EXIT_FAILURE;

    ConfigureADC();
    printf("ADC Configured\n");

    int x = 1;
    while (x == 1)
    {
        printf("voltage: %f", GetVoltageValue());
        usleep(125);
    }
}