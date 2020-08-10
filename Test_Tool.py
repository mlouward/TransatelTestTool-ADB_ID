import subprocess
import os
import sys
import time
from datetime import datetime


def check_adb():
    if not os.path.isdir("platform-tools"):
        subprocess.run(["DL_Adb.bat"])

def get_number_to_imsi(path="simInfos.csv", sep=';'):
    """ Reads the file with sim infos and creates a dictionary
    with numbers as key and imsis as values for all the phones in the file.

    Args:
        path: The path of the file with the phone infos.
        sep: The separator used in the file.
        ignore: whether to ignore the first line of the file or not.

    Returns:
        Number To Imsi dictionary
    """
    number_to_imsi = dict()
    with open(path) as f:
        f.readline() # used to ignore header of csv.
        for line in f:
            l = line.rstrip().split(sep)
            if len(l) == 3:
                number_to_imsi.update({l[1]:l[2]}) # (Dict are insertion ordered since Python 3.6)
    return number_to_imsi

def get_imsi_to_id():
    """ Reads the info on plugged in devices and creates a dictionary
    with imsis as key and ADB ids as values.

    Returns:
        IMSI To Id dictionary
    """
    subprocess.run(["getPhoneId.bat"])
    imsi_to_id = dict() # IMSI and corresponding id
    try:
        with open("imsiList.txt") as f:
            for line in f:
                l = line.rstrip().split(';')
                imsi_to_id.update({l[0]:l[1]})
        os.remove("imsiList.txt")
        return imsi_to_id
    except:
        print("No devices has been plugged in. The program will exit.", file=sys.stderr)
        sys.exit(1)

def get_dictionaries():
    """ Returns the dictionaries needed.
    """
    number_to_imsi = get_number_to_imsi()
    imsi_to_id = get_imsi_to_id()
    #id_to_number = {imsi_to_id[v]:k for k, v in number_to_imsi.items() if v in imsi_to_id.keys()}
    return number_to_imsi, imsi_to_id

def get_apn(index, numeric):
    """ Gets the APN list for a phone.

    Args:
        index: The index of the phone to query APNs
        numeric: The MCC + MNC code for which to query.
    """
    num = tuple(number_to_imsi.items())[int(index) - 1][0]
    id = imsi_to_id[number_to_imsi[num]]
    subprocess.run(["getApn.bat", id, str(numeric)])

def moc_routine(n, call_duration, index_a, index_b):
    """ Performs MOC from phone A to B.

    Args:
        n: Number of calls to make from A to B.
        call_duration: The total duration, starting when phone A dials.
        index_a: index of phone A in simInfos.csv
        index_b: index of phone B in simInfos.csv, or a phone number to call.
    """
    print("\n[{}] Beginning MOC routine...\n".format(str(datetime.now().strftime("%H:%M:%S"))))
    to = int(n) * int(call_duration) + 20
    try:
        num_a = tuple(number_to_imsi.items())[int(index_a) - 1][0]
    except:
        print("Selected phone is not plugged in")
    # If the argument is longer than 2 digits, it is a phone number and not an
    # index, so we call this number.  Else,
    # we assume it is the index of a phone
    if len(index_b) > 2:
        num_b = index_b
    else:
        num_b = tuple(number_to_imsi.items())[int(index_b) - 1][0]
    try:
        id_a = imsi_to_id[number_to_imsi[num_a]]
    except:
        print(f"Selected phone is not plugged in ({num_a})", file=sys.stderr)
        return
    try:
        subprocess.run(["moc.bat", id_a, num_b, num_a, call_duration, str(n)], timeout=to)
    except:
        with open("logs\\MOClog.txt", "a") as f:
            f.write("[{}] Call unsuccessful (process timed out) (FROM: {}, TO: {}, NB: {}, DURATION: {}sec.)\n\n".format(
                str(datetime.now().strftime("%d/%m/%Y-%H:%M:%S")), num_a, num_b, n, call_duration))

def mtc_routine(n, call_duration, index_a, index_b):
    """ Performs MTC between phones A and B. A calls B, B answers
    and A hangs up after detecting B has answered. 1 sec delay
    between consecutive calls and 1 second before picking up.

    Args:
        n: Number of calls to make from each phone.
        call_duration: Duration of the call.
        index_a: the index of the phone calling in simInfos.csv
        index_b: the index of the phone called in simInfos.csv
    """
    print("\n[{}] Beginning MTC routine...\n".format(str(datetime.now().strftime("%H:%M:%S"))))
    to = int(n) * int(call_duration) + 20
    num_a = tuple(number_to_imsi.items())[index_a - 1][0]
    num_b = tuple(number_to_imsi.items())[index_b - 1][0]
    try:
        id_a = imsi_to_id[number_to_imsi[num_a]]
        id_b = imsi_to_id[number_to_imsi[num_b]]
    except:
        print(f"Selected phone is not plugged in ({num_a})", file=sys.stderr)
        return
    for i in range(n):
        try:
            subprocess.run(["mtc.bat", id_a, id_b, num_a, num_b, call_duration, str(i + 1)], timeout=to)
        except:
            with open("logs\\MTClog.txt", "a") as f:
                f.write("[{}] Call unsuccessful (process timed out) (FROM: {}, TO: {}, NB: {}, DURATION: {}sec.)\n\n".format(
                    str(datetime.now().strftime("%d/%m/%Y-%H:%M:%S")), num_a, num_b, n, call_duration))
    try:
        with open("logs\\MTClog.txt", 'a') as f:
            f.write("\n")
    except:
        print("Log file not found.", file=sys.stderr)
        return

def sms_routine(n, text, index_a, index_b):
    """ Sends SMS of varying length from phone A to B then from phone B to A.
    A and B are the only phones plugged in.

    Args:
        n: Number of SMS to send from each phone.
        text: the text to be sent via SMS.
        index_a: the index of the sender in simInfos.csv
        index_b: the index of the receiver in simInfos.csv, or a phone number to send to.
    """
    print("\n[{}] Beginning SMS routine...\n".format(str(datetime.now().strftime("%H:%M:%S"))))
    num_a = tuple(number_to_imsi.items())[int(index_a) - 1][0]
    text = text.replace('"', "\\'")
    if len(index_b) > 2:
        num_b = index_b
    else:
        num_b = tuple(number_to_imsi.items())[int(index_b) - 1][0]
    try:
        id_a = imsi_to_id[number_to_imsi[num_a]]
    except:
        print(f"Selected phone is not plugged in ({num_a})", file=sys.stderr)
        return
    for i in range(n):
        try:
            subprocess.run(["sms.bat", id_a, num_b, num_a, str(i + 1), text], timeout=int(n))
        except:
            with open("logs\\SMSlog.txt", "a") as f:
                f.write("[{}] SMS routine unsuccessful (process timed out) (FROM: {}, TO: {}, NB: {}, TEXT: {})\n\n".format(
                    str(datetime.now().strftime("%d/%m/%Y-%H:%M:%S")), num_a, num_b, n, text))
    try:
        with open("logs\\SMSlog.txt", 'a') as f:
            f.write("\n")
    except:
        print("Log file not found.", file=sys.stderr)
        return

def data_routine(n, url=r"http://www.google.com", index=1):
    """Performs a data test with the desired phone, by opening the default browser
    and loading the given URL (must be a complete URL).

    Args:
        n: the number of times to open the page
        url: the url to open.
        index: the index of the phone to use in simInfos.csv
    """
    # 1 second per request and 10 seconds to unlock phone and open browser.
    to = int(n) + 10
    num = tuple(number_to_imsi.items())[index - 1][0]
    try:
        id = imsi_to_id[number_to_imsi[num]]
    except:
        print(f"Selected phone is not plugged in ({num})", file=sys.stderr)
        return
    try:
        subprocess.run(["data.bat", id, url, num, str(n)], timeout=to)
    except:
        with open("logs\\datalog.txt", "a") as f:
            f.write("[{}] Data test unsuccessful (process timed out) (PHONE: {}, URL: {})\n\n".format(
                str(datetime.now().strftime("%d/%m/%Y-%H:%M:%S")), num, url))

def speedtest_routine(index, size='20'):
    """ Performs a speedtest by downloading a 50MB file from phone A.

    Args:
        index_a: the index of the phone to test in simInfos.csv
        size: the size of the file to download. Must be one of (20, 50, 100)
    """
    try:
        assert size in ('10', '20', '50', '100')
    except:
        print("The file size is not compatible. Please use one of (10, 20, 50, 100).", file=sys.stderr)
        return
    to = int(size) * 10 # (100Kio/s is the limit for timeout)
    print("\n[{}] Beginning speedtest...\n".format(str(datetime.now().strftime("%H:%M:%S"))))
    num = tuple(number_to_imsi.items())[index - 1][0]
    try:
        id = imsi_to_id[number_to_imsi[num]]
    except:
        print(f"Selected phone is not plugged in ({num})", file=sys.stderr)
        return
    try:
        subprocess.run(["speedtest.bat", id, num, size], timeout=to)
    except:
        with open("logs\\speedtestlog.txt", "a") as f:
            f.write("[{}] Speedtest unsuccessful (process timed out) (PHONE: {}, SIZE: {})\n\n".format(
                str(datetime.now().strftime("%d/%m/%Y-%H:%M:%S")), num, size))
    format = "%d/%m/%Y-%H:%M:%S,%f"
    try:
        with open("platform-tools\\elapsed.txt") as f:
            start = datetime.strptime(f.readline().strip(), format)
            end = datetime.strptime(f.readline().strip(), format)
        s = str((end - start).seconds)
        us = str((end - start).microseconds)
        t = float(s + "." + us) # get precise time with milliseconds
        speed = int(size) / t
        os.remove("platform-tools\\elapsed.txt")
        print("[{}] Download complete. Average download speed: {}MB/s".format(
            str(datetime.now().strftime("%H:%M:%S")), round(speed, 2))) 
    except IOError:
        print("File not found. The program will be terminated. Please verify the plugged in devices.", file=sys.stderr)
        sys.exit(2)
    try:
        with open("logs\\speedtestlog.txt", 'a') as f:
            f.write("Average speed: {}MB/sec.\n\n".format(round(speed, 2)))
    except:
        print("Log file not found.", file=sys.stderr)
        return

def ping_routine(index, address, n, size):
    """
    Performs a ping test on a connected phone

    Args:
        n: Number of packets to send
        address: The IP address to ping
        index: The index of the phone to use in simInfos.csv
        size: The size of the packets in bytes
    """
    to = int(n) + 10
    num = tuple(number_to_imsi.items())[index - 1][0]
    try:
        id = imsi_to_id[number_to_imsi[num]]
    except:
        print(f"Selected phone is not plugged in ({num})", file=sys.stderr)
        return
    print("\n[{}] Beginning Ping routine...\n".format(str(datetime.now().strftime("%H:%M:%S"))))
    try:
        subprocess.run(["ping.bat", id, address, num, n, size], timeout=to)
    except:
        with open("logs\\pinglog.txt", "a") as f:
            f.write("[{}] Ping test unsuccessful (process timed out) (PHONE: {}, ADDRESS: {}, NB: {}, SIZE: {})\n\n".format(
                str(datetime.now().strftime("%d/%m/%Y-%H:%M:%S")), num, address, n, size))

def airplane_routine(index, duration):
    to = int(duration) + 10
    num = tuple(number_to_imsi.items())[index - 1][0]
    try:
        id = imsi_to_id[number_to_imsi[num]]
    except:
        print(f"Selected phone is not plugged in ({num})", file=sys.stderr)
        return
    print("\n[{}] Beginning Airplane routine...\n".format(str(datetime.now().strftime("%H:%M:%S"))))
    try:
        subprocess.run(["airplane.bat", id, duration, num], timeout=to)
    except:
        with open("logs\\airplanelog.txt", "a") as f:
            f.write("[{}] Airplane test unsuccessful (process timed out) (PHONE: {}, DURATION: {})\n\n".format(
                str(datetime.now().strftime("%d/%m/%Y-%H:%M:%S")), num, duration))
         

def get_test_list(path="testsToPerform.csv", header=True):
    """ Performs different tests according to a list defined by the user.

    Args:
        path: List of the file containing the tests to do (in order)
        header: whether the test file has a header or not.
    """
    print("[{}] Reading test file...".format(str(datetime.now().strftime("%H:%M:%S"))))
    with open(path) as f:
        if header: f.readline()
        for i, line in enumerate(f):
            l = line.rstrip().split(';')
            if l[0].lower() == "moc":
                moc_routine(l[1], l[2], l[3], l[4])
                time.sleep(int(l[5]))
            elif l[0].lower() == "sms":
                sms_routine(int(l[1]), l[2], l[3], l[4])
                time.sleep(int(l[5]))
            elif l[0].lower() == "mtc":
                mtc_routine(int(l[1]), l[2], int(l[3]), int(l[4]))
                time.sleep(int(l[5]))
            elif l[0].lower() == "data":
                if l[2] == '': data_routine(int(l[1]), index=int(l[3]))
                else: data_routine(int(l[1]), l[2], int(l[3]))
                time.sleep(int(l[4]))
            elif l[0].lower() == "speedtest":
                speedtest_routine(int(l[1]), l[2])
                time.sleep(int(l[3]))
            elif l[0].lower() == "ping":
                ping_routine(int(l[1]), l[3], l[4], l[5])
                time.sleep(int(l[4]))
            elif l[0].lower() == "airplane":
                airplane_routine(int(l[1]), l[2])
                time.sleep(int(l[3]))
            else:
                print("\n'{}' is not a valid test (line {}).".format(l[0], i + 1))
    print("\nTests are over.")
    return True

if __name__ == '__main__':
    check_adb()
    number_to_imsi, imsi_to_id = get_dictionaries()
    get_test_list(header=False)
