from run.email_service import send_fail
import argparse

parser = argparse.ArgumentParser()
parser.add_argument("-e", "--email", required=True, type=str, help="Send email")
args = parser.parse_args()

send_fail(args.email, "stdout.log", "stderr.log")
