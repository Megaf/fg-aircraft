/^name \"ca\-/{
    item=$2
    gsub("\"","",item)
print "\n<!-- Caution light "item" -->"

print "	<animation>"
print "		<type>select</type>"
print "		<object-name>"item"</object-name>"
print "		<condition>"
print "            <or>"
print "                <property>sim/model/f15/lights/master-test-lights</property>"
print "               <property>sim/model/f15/lights/"item"</property>"
print "            </or>"
print "				<greater-than>"
print "					<property>/fdm/jsbsim/systems/electrics/ac-essential-bus1</property>"
print "					<value>0</value>"
print "				</greater-than>"
print "		</condition>"
print "	</animation>"
print "	<animation>"
print "		<object-name>"item"</object-name>"
print "		<type>material</type>"
print "		<emission> "
print "			<factor-prop>sim/model/f15/controls/lighting/index-norm</factor-prop>"
print "			<red>1</red>"
print "			<green>1</green>"
print "			<blue>1</blue>"
print "		</emission>"
print "	</animation>"
print ""
print ""
}