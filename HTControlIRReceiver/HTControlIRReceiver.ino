#include <IRremoteInt.h>
#include <IRremote.h>
#include <SoftwareSerial.h>

//#define DEBUG true
#define IR_RECEIVE_PIN 12
#define WAIT_NEXT_COMMAND_LED_PIN 7
#define soundBarPowerCommand 0x100C //Power key on soundbar remote
#define HTPowerOnCommand 0x10DC //"Night mode on" on soundbar remote
#define HTPowerOffCommand 0x10DE //"Auto volume on" on soundbar remote
#define waitForSecondCommandTimeout 5 //time to wait for second command in seconds
#define RS485_RECEIVE_PIN 10
#define RS485_TRANSMIT_PIN 11

IRrecv irrecv(IR_RECEIVE_PIN);
IRsend irsend;
decode_results results;

SoftwareSerial RS485Serial(RS485_RECEIVE_PIN, RS485_TRANSMIT_PIN); // RX, TX

bool waitingForSecondCommand = false; 
int timer = 0; //timeout timer for the second command
bool DEBUGPRINT = true;

void setup()
{
	//if (DEBUGPRINT) 
	Serial.begin(9600);
	while (!Serial) {
		; // wait for serial port to connect. Needed for native USB port only
	}

	irrecv.enableIRIn(); // Start the receiver
	/*irrecv.blink13(true);*/	
	pinMode(WAIT_NEXT_COMMAND_LED_PIN, OUTPUT);
	digitalWrite(WAIT_NEXT_COMMAND_LED_PIN, LOW);
	
	debugPrint("HTControl IR receiver started");
	
	RS485Serial.begin(9600);   // set the data rate 
	digitalWrite(3, HIGH);  // Init Transceiver   
}

void loop() {
	if (irrecv.decode(&results)) {
		//printReceivedIR(results);
		
		results.value &= 0xfeffff; //mask out title bit as per https://learn.adafruit.com/using-an-infrared-library/hardware-needed

		if(!waitingForSecondCommand)
		{		
			if (results.decode_type == RC6 && results.value == soundBarPowerCommand)
			{			
				debugPrint("Power off at soundbar remote pressed");
				waitingForSecondCommand = true;
				digitalWrite(WAIT_NEXT_COMMAND_LED_PIN, HIGH);
				timer = 0;
				//delay(100);			
			}
		}
		else
		{
			if (results.decode_type == RC6 && results.value == HTPowerOnCommand)
			{				
				debugPrint("HT on at soundbar remote pressed");
				waitingForSecondCommand = false;
				digitalWrite(WAIT_NEXT_COMMAND_LED_PIN, LOW);
				RS485send("HTON");
				//delay(100);
			}
			else if (results.decode_type == RC6 && results.value == HTPowerOffCommand)
			{
				debugPrint("HT off at soundbar remote pressed");
				waitingForSecondCommand = false;
				digitalWrite(WAIT_NEXT_COMMAND_LED_PIN, LOW);
				RS485send("HTOFF");				
				//delay(100);
			}
		}

		irrecv.resume(); // Receive the next value
	}	

	if (waitingForSecondCommand)
	{
		delay(10);
		timer += 10;
		if (timer > waitForSecondCommandTimeout * 1000)
		{
			debugPrint("Timeout when waiting for second command");
			waitingForSecondCommand = false;
			digitalWrite(WAIT_NEXT_COMMAND_LED_PIN, LOW);
		}
	}
}

void RS485send(String dataString)
{
	RS485Serial.print(dataString);
	debugPrint(dataString + " sent");
}

void debugPrint(String text)
{
	//if (DEBUGPRINT) 
	Serial.println(text);
}

//print received IR command for debugging
void printReceivedIR(decode_results results) {
	Serial.print("--- Received: ---");
	if (results.decode_type == NEC) {
		Serial.print("NEC: ");
	}
	else if (results.decode_type == SONY) {
		Serial.print("SONY: ");
	}
	else if (results.decode_type == RC5) {
		Serial.print("RC5: ");
	}
	else if (results.decode_type == RC6) {
		Serial.print("RC6: ");
	}
	else if (results.decode_type == UNKNOWN) {
		Serial.print("UNKNOWN: ");
	}

	Serial.print("value: ");
	Serial.println(results.value, HEX);
	Serial.print("address: ");
	Serial.println(results.address, HEX);
	Serial.print("bits: ");
	Serial.println(results.bits);
	Serial.print("rawlen: ");
	Serial.println(results.rawlen, HEX);
	Serial.print("overflow: ");
	Serial.println(results.overflow, HEX);
	Serial.print("--------------");
}

