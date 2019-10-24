import serial
import datetime, time

ser = serial.Serial('/dev/cu.SLAB_USBtoUART')

def write(serial, text):
    text = bytes(text, 'ascii')
    serial.write(text + b'\r')


def sendCommand(serial, command, value=None):
    write(serial, command)
    
    if value is not None:
        write(serial, value)
    
    out = serial.read_until(b'OK\r').decode('ascii')
    return out.replace('OK\r', '')

a = datetime.datetime.now()
b = datetime.datetime.now()

sendCommand(ser, 'VOLT030')
sendCommand(ser, 'CURR050')

while True:
    out = sendCommand(ser, 'GETD')

    print(out)

ser.close()