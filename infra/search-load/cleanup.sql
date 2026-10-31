\set ON_ERROR_STOP on

begin;

create temp table load_users as select id from users where email::text like '%@carga.vitrio.local';
create temp table load_providers as select id from providers where user_id in (select id from load_users);

delete from favorites where provider_id in (select id from load_providers) or user_id in (select id from load_users);
delete from provider_services where provider_id in (select id from load_providers);
delete from provider_categories where provider_id in (select id from load_providers);
delete from providers where id in (select id from load_providers);
delete from users where id in (select id from load_users);

commit;

analyze providers;

select count(*) as remaining_load_users from users where email::text like '%@carga.vitrio.local';
