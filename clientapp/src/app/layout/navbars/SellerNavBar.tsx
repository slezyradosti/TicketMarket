import { observer } from "mobx-react";
import { useState } from "react";
import { Dropdown, DropdownItem, DropdownMenu, Menu, MenuItem, MenuMenu } from "semantic-ui-react";

function SellerNavBar() {
    const [activeItem, setActiveItem] = useState<string>();

    const handleItemClick = (name: string) => setActiveItem(name);

    return (
        <>
            <Menu color='blue' inverted style={{ borderRadius: '0' }}>
                <MenuItem
                    name='My Events'
                    active={activeItem === 'my_events'}
                    onClick={() => handleItemClick('my_events')}
                />
                <MenuItem
                    name='My Discounts'
                    active={activeItem === 'my_discounts'}
                    onClick={() => handleItemClick('my_discounts')}
                />

                <MenuMenu position='right'>
                    <Dropdown
                        item
                        text='Account'
                        icon='user outline'
                        floating
                        labeled
                        button
                        className='icon'
                    >
                        <DropdownMenu>
                            <DropdownItem
                                text='Log out'
                            />
                        </DropdownMenu>
                    </Dropdown>
                </MenuMenu>
            </Menu >
        </>
    );
}

export default observer(SellerNavBar);