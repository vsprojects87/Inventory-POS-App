
In Roles tables, we have on delete cascade which means if we delete the role from database the role assigned to
user will also be deleted

we have roleid and userid mapping so data of specific role mapping will be deleted once the role is deleted

even if we give policy in view we still need to assgin policy to control methods as well otherswise through
url the action method can be access even if we hide it on views