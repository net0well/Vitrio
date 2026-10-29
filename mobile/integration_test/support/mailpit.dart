import 'dart:convert';
import 'dart:io';

const _mailpit = String.fromEnvironment('MAILPIT_URL', defaultValue: 'http://10.0.2.2:8025');

Future<String> confirmationCodeFor(String email) async {
  final client = HttpClient();
  try {
    for (var attempt = 0; attempt < 30; attempt++) {
      final request = await client.getUrl(Uri.parse('$_mailpit/api/v1/search?query=${Uri.encodeQueryComponent('to:$email')}'));
      final response = await request.close();
      final body = jsonDecode(await response.transform(utf8.decoder).join()) as Map<String, Object?>;
      for (final message in (body['messages'] as List<Object?>? ?? const []).cast<Map<String, Object?>>()) {
        final code = RegExp(r'^(\d{6})').firstMatch(message['Subject']! as String)?.group(1);
        if (code != null) {
          return code;
        }
      }

      await Future<void>.delayed(const Duration(milliseconds: 500));
    }
  } finally {
    client.close();
  }

  throw StateError('No confirmation code arrived for $email.');
}
