import subprocess
import sys
import os


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
        return

def add_apn(imsi, name, mcc, mnc, apn):
    """ Adds the APN with the selected parameters for
        the selected phone.

    Args:
        imsi: The imsi of the phone to add the APN to.
        name: The displayed name of the new APN.
        mcc: MCC of the new APN.
        mnc: MNC of the new APN.
        apn: The address of the new APN.
    """
    imsi_to_id = get_imsi_to_id()
    id = imsi_to_id[imsi]
    subprocess.run(["addApn.bat", id, name, mcc, mnc, apn, imsi])

if __name__ == "__main__":
    add_apn(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5])
