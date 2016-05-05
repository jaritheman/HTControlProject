#include <SoftwareSerial.h>

#define RS485_IR_RECEIVER_RECEIVE_PIN 11
#define RS485_IR_RECEIVER_TRANSMIT_PIN 12

//#define RS485_PROJECTOR_RECEIVE_PIN ?
//#define RS485_PROJECTOR_TRANSMIT_PIN ?

#define RS232_HDMI_SWITCH_RECEIVE_PIN 5                              
#define RS232_HDMI_SWITCH_TRANSMIT_PIN 6

SoftwareSerial RS485_ir_receiver_serial(RS485_IR_RECEIVER_RECEIVE_PIN, RS485_IR_RECEIVER_TRANSMIT_PIN); // RX, TX
SoftwareSerial RS232_HDMI_switch_serial(RS232_HDMI_SWITCH_RECEIVE_PIN, RS232_HDMI_SWITCH_TRANSMIT_PIN); // RX, TX

String cmd;

void setup()
{
	Serial.begin(9600);
	while (!Serial) {
		; // wait for serial port to connect. Needed for native USB port only
	}

	RS485_ir_receiver_serial.begin(9600);
	RS485_ir_receiver_serial.setTimeout(100);

	RS232_HDMI_switch_serial.begin(9600); //commenting this out works?
	
	pinMode(3, OUTPUT);
	digitalWrite(3, LOW);  // Enable RS485 Receive
	
	debugPrint("HTControl main unit started");
}

void loop()
{	
	RS485_ir_receiver_serial.listen();
	if (RS485_ir_receiver_serial.available())
	{		
		cmd = RS485_ir_receiver_serial.readString();
		//debugPrint("Received: " + cmd);
		if (cmd == "HTON")
		{
			debugPrint("HTON Received");
			RS232_HDMI_switch_serial.print("POWER 01\n\r");
		}

		if (cmd == "HTOFF")
		{
			debugPrint("HTOFF Received");
			RS232_HDMI_switch_serial.print("POWER 00\n\r");
		}
		delay(10);
	}
}

void debugPrint(String text)
{
	Serial.println(text);
}