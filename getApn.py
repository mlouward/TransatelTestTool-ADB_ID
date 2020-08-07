import subprocess
import sys
import os


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

if __name__ == "__main__":
    number_to_imsi, imsi_to_id = get_dictionaries()
    get_apn(sys.argv[1], sys.argv[2])