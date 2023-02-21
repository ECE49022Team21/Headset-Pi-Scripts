from smbus import SMBus

ADC_Address = 0x54
bus = SMBus(1)

while True:
    