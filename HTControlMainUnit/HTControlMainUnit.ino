#include <SoftwareSerial.h>

#define RS485_IR_RECEIVER_RECEIVE_PIN 11
#define RS485_IR_RECEIVER_TRANSMIT_PIN 12

#define RS485_PROJECTOR_RECEIVE_PIN 9
#define RS485_PROJECTOR_TRANSMIT_PIN 10

#define RS232_HDMI_SWITCH_RECEIVE_PIN 5                              
#define RS232_HDMI_SWITCH_TRANSMIT_PIN 6

#define USB_HTPC_RECEIVE_PIN 7                              
#define USB_HTPC_TRANSMIT_PIN 8


SoftwareSerial RS485_ir_receiver_serial(RS485_IR_RECEIVER_RECEIVE_PIN, RS485_IR_RECEIVER_TRANSMIT_PIN); // RX, TX
SoftwareSerial RS232_HDMI_switch_serial(RS232_HDMI_SWITCH_RECEIVE_PIN, RS232_HDMI_SWITCH_TRANSMIT_PIN); // RX, TX
SoftwareSerial RS485_Projector_serial(RS485_PROJECTOR_RECEIVE_PIN, RS485_PROJECTOR_TRANSMIT_PIN); // RX, TX
SoftwareSerial USB_HTPC_serial(USB_HTPC_RECEIVE_PIN, USB_HTPC_TRANSMIT_PIN); // RX, TX

String cmd;

void setup()
{
	Serial.begin(9600);
	while (!Serial) {
		; // wait for serial port to connect. Needed for native USB port only
	}

	RS485_ir_receiver_serial.begin(9600);
	RS485_ir_receiver_serial.setTimeout(100);
	
	RS232_HDMI_switch_serial.begin(9600);

	RS485_Projector_serial.begin(9600);	
	
	USB_HTPC_serial.begin(9600);
	
	pinMode(3, OUTPUT);
	digitalWrite(3, LOW);  // Enable RS485 Receive (RS485 from IR receiver)

	pinMode(2, OUTPUT);
	digitalWrite(2, HIGH);  // Enable RS485 Transmit (RS485 from to projector)
	
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
			delay(100);
			USB_HTPC_serial.println("HTON");
			delay(100);
			RS485_Projector_serial.print("POWER 01\n\r"); //todo: add actual command for power on Panasonic PT-AE2000
		}

		if (cmd == "HTOFF")
		{
			debugPrint("HTOFF Received");
			USB_HTPC_serial.println("HTOFF");
			delay(10000); //wait for 10 seconds to allow used to cancel						
			RS485_Projector_serial.print("POWER 00\n\r"); //todo: add actual command for power off Panasonic PT-AE2000
			delay(100);
			RS232_HDMI_switch_serial.print("POWER 00\n\r");			
		}
		delay(10);
	}
}

void debugPrint(String text)
{
	Serial.println(text);
}
