import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from email.mime.base import MIMEBase
from email import encoders

gmail_user = "applicationds317@gmail.com"
gmail_pass = "@pplicationDs317"

def create_attachment(file_name):
    file = open(file_name, 'rb')
    payload = MIMEBase('text', 'csv')
    payload.set_payload(file.read())
    file.close()
    encoders.encode_base64(payload)
    payload.add_header('Content-Disposition', 'attachment', filename = file_name)
    return payload


def create_mail(email, output_file):
    message = MIMEMultipart()
    message["From"] = gmail_user
    message["To"] = email
    message["Subject"] = "Benchmark results"
    message.attach(MIMEText("These are the results from benchmarking", 'plain', 'utf-8'))

    file_prefix = output_file.split('.csv')[0]
    message.attach(create_attachment(file_prefix + '_stats_pkg_power.csv'))
    message.attach(create_attachment(file_prefix + '_stats_ram_power.csv'))
    message.attach(create_attachment(file_prefix + '_stats_run_time.csv'))

    return message


def send_results(email, output_file):
    server = smtplib.SMTP("smtp.gmail.com", 587)
    server.starttls()
    server.login(gmail_user, gmail_pass)

    message = create_mail(email, output_file)
    server.sendmail(gmail_user, email, message.as_string())

    server.close()
