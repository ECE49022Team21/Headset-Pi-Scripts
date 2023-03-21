gcc *.c ./I2Clib/*.c ./I2Clib/GPIOlib/*.c ./I2Clib/MicroSleepLib/*.c
mv ./a.out ./Headset.run
rm -r 'C#Build'
cd './C# Google Cloud Scripts/SpeechToTextAgent'
dotnet publish --runtime linux-arm --self-contained --framework net6.0
mv ./bin/Debug/net6.0/linux-arm/publish ../../'C#Build'