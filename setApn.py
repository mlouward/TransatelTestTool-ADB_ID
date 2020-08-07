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
        sys.exit(1)

def set_apn(imsi, apn_id):
    """ Sets the APN with the selected ID as the default for the
        selected phone.

    Args:
        imsi: The imsi of the phone to set the default APN.
        apn_id: The id of the APN to set as default.        
    """
    imsi_to_id = get_imsi_to_id()
    id = imsi_to_id[imsi]
    subprocess.run(["setDefaultApn.bat", id, apn_id, imsi])

if __name__ == "__main__":
    set_apn(sys.argv[1], sys.argv[2])
