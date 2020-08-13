import subprocess
import sys
import os
import time


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
                number_to_imsi.update({l[1]:l[2]}) # Dict are insertion ordered since Python 3.6
    return number_to_imsi

def get_imsi_to_id():
    """ Reads the info on plugged in devices and creates a dictionary
    with imsis as key and ADB ids as values.

    Returns:
        IMSI To Id dictionary
    """
    subprocess.run(["getPhoneId.bat"])
    imsi_to_id = dict() # IMSI and corresponding ADB id
    imsi_to_sub = dict() # IMSI and corresponding SubId
    try:
        with open("imsiList.txt") as f:
            for line in f:
                l = line.rstrip().split(';')
                imsi_to_id.update({l[0]:l[1]})
        os.remove("imsiList.txt")
        with open("imsiToSubId.txt") as f:
            for line in f:
                l = line.rstrip().split(';')
                imsi_to_sub.update({l[0]:l[1]})
        os.remove("imsiToSubId.txt")
        return imsi_to_id, imsi_to_sub
    except:
        print("No devices has been plugged in. The program will exit.", file=sys.stderr)
        return

def get_dictionaries():
    """ Returns the dictionaries needed.
    """
    number_to_imsi = get_number_to_imsi()
    imsi_to_id, imsi_to_sub = get_imsi_to_id()
    return number_to_imsi, imsi_to_id, imsi_to_sub

def check_root(index):
    """ Adds the APN with the selected parameters for
        the selected phone.

    Args:
	index: The list of indexes of the phones to check.
    """
    try:
        with open("rootList.txt", 'w') as f:
            pass
    except:
        print("rootList.txt not found", file=sys.stderr)
    # we use range for when the indexes of the phones are skipped
    # ([1, 2, 5] instead of [1, 2, 3] for example)
    for i in range(len(index)):
        num = tuple(number_to_imsi.items())[int(i)][0]
        print(num)
        try:
            id = imsi_to_id[number_to_imsi[num]]
            print(id)
            subprocess.run(["checkRoot.bat", id])
        except: # Triggers when a phone is not plugged in.
            with open("rootList.txt", 'a') as f:
                f.write(";;false\n")
            
if __name__ == "__main__":
    number_to_imsi, imsi_to_id, imsi_to_sub = get_dictionaries()
    check_root(sys.argv[1:])
